using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Common.Presentation.Api.Configuration.Options;
using Common.Presentation.Api.Configuration.Validators;
using Common.Presentation.Api.Extensions;
using Common.Presentation.Api.Middlewares.ApiKeyHandling;
using Common.Presentation.Api.Middlewares.CorrelationHandling;
using Common.Presentation.Api.Middlewares.ExceptionHandling;
using Common.Presentation.Api.Middlewares.GlobalObservabilityHandling;
using Common.Presentation.Api.Middlewares.MaintenanceHandling;
using Common.Presentation.Api.Middlewares.RequestLoggingHandling;
using Common.Presentation.Api.Middlewares.TimeoutHandling;
using Common.Presentation.Api.Swagger;
using Common.Presentation.Contract;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Common.Presentation;

/// <summary>
/// Shared HTTP host composition for API processes.
/// </summary>
public static class DependencyInjection
{
    private const string DevUserSecurityId = "DevUser";
    private const string IngestionApiKeySecurityId = "IngestionApiKey";

    public static IServiceCollection AddServiceIdentity(this IServiceCollection services,
                                                        IConfiguration configuration,
                                                        Assembly assembly,
                                                        string serviceNamespaceFallback,
                                                        string serviceComponentFallback,
                                                        string serviceVersionFallback = "Unknown")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceNamespaceFallback, nameof(serviceNamespaceFallback));
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceComponentFallback, nameof(serviceComponentFallback));
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceVersionFallback, nameof(serviceVersionFallback));

        services.TryAddSingleton(_ =>
        {
            var serviceNamespace = configuration["Name"] ?? serviceNamespaceFallback;
            var serviceComponent = configuration["Component"] ?? serviceComponentFallback;
            var serviceVersion = assembly.GetName().Version?.ToString() ?? serviceVersionFallback;
            var serviceName = $"{serviceNamespace}.{serviceComponent}";

            return new ServiceIdentity(serviceNamespace, serviceName, serviceComponent, serviceVersion);
        });

        return services;
    }

    /// <summary>
    /// Registers API versioning and OpenAPI for HTTP hosts.
    /// CQRS and validators are registered via product <c>AddApplication</c> and <c>AddFramework</c>.
    /// </summary>
    /// <param name="serviceName">Fallback service name when <c>ApplicationName</c> is not configured.</param>
    public static IServiceCollection AddCommonApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
                .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddCommonApiSwagger(configuration)
                .AddApiMiddleware(configuration)
                .AddApiHttpObservabilityMiddleware(configuration)
                .AddApiCorrelationMiddleware(configuration)
                .AddApiExceptionHandlingMiddleware(configuration)
                .AddApiMaintenanceModeMiddleware(configuration)
                .AddApiRequestLoggingMiddleware(configuration);

        return services;
    }

    private static IServiceCollection AddCommonApiSwagger(this IServiceCollection services, IConfiguration configuration, Assembly? documentationAssembly = null)
    {
        services.AddOptions<SwaggerOptions>()
                .Bind(configuration.GetSection(SwaggerOptions.SectionName))
                .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<SwaggerOptions>, SwaggerOptionsValidator>();

        var swaggerOptions = configuration.GetSection(SwaggerOptions.SectionName).Get<SwaggerOptions>() ?? new SwaggerOptions();

        if (!swaggerOptions.Enabled) return services;

        var xmlAssembly = documentationAssembly ?? Assembly.GetEntryAssembly();

        services.AddEndpointsApiExplorer();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>>(sp => new VersionedSwaggerGenOptions(
                                                                    sp.GetRequiredService<IApiVersionDescriptionProvider>(),
                                                                    sp.GetRequiredService<IOptions<SwaggerOptions>>(),
                                                                    sp.GetRequiredService<ServiceIdentity>()));

        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".", StringComparison.Ordinal) ?? type.Name);

            if (swaggerOptions.IncludeDevUserHeader)
                options.AddSecurityDefinition(DevUserSecurityId, new OpenApiSecurityScheme
                {
                    Name = "X-User-Id",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Development-only user id (snowflake). Requires Infrastructure:Acl:DevAuthEnabled.",
                });

            if (swaggerOptions.IncludeIngestionApiKey)
                options.AddSecurityDefinition(IngestionApiKeySecurityId, new OpenApiSecurityScheme
                {
                    Name = "X-Api-Key",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Collector ingestion API key (Ingestion:ApiKey).",
                });

            if (swaggerOptions.IncludeDevUserHeader)
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = DevUserSecurityId } },
                        Array.Empty<string>()
                    },
                });

            if (swaggerOptions.IncludeIngestionApiKey)
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = IngestionApiKeySecurityId } },
                        Array.Empty<string>()
                    },
                });

            if (xmlAssembly is not null)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{xmlAssembly.GetName().Name}.xml");

                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });

        return services;
    }
    
    private static IServiceCollection AddApiMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddApiKeyMiddleware(configuration)
                       .AddApiRequestTimeoutsMiddleware(configuration);
    }
    private static IServiceCollection AddApiKeyMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ApiKeyOptions>, ApiKeyOptionsValidator>());
        services.AddOptions<ApiKeyOptions>().BindConfiguration(ApiKeyOptions.SectionName).ValidateOnStart();
        services.AddTransient<ApiKeyValidator>();
        services.AddTransient<ApiKeyMiddleware>();
        services.AddSingleton<ApiKeyOptionsValidator>();

        return services;
    }
    private static IServiceCollection AddApiRequestTimeoutsMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiRequestTimeoutOptions>().BindConfiguration(ApiRequestTimeoutOptions.SectionName);

        var s = configuration.GetSection(ApiRequestTimeoutOptions.SectionName).Get<ApiRequestTimeoutOptions>() ?? new ApiRequestTimeoutOptions();

        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(s.DefaultSeconds)
            };

            options.AddPolicy(RequestTimeoutPolicies.Fast, TimeSpan.FromSeconds(s.FastSeconds));
            options.AddPolicy(RequestTimeoutPolicies.Standard, TimeSpan.FromSeconds(s.StandardSeconds));
            options.AddPolicy(RequestTimeoutPolicies.LongRunning, TimeSpan.FromSeconds(s.LongRunningSeconds));
            options.AddPolicy(RequestTimeoutPolicies.ThirdParty, TimeSpan.FromSeconds(s.ThirdPartySeconds));
        });

        return services;
    }
    private static IServiceCollection AddApiHttpObservabilityMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<HttpMetrics>();
        services.AddTransient<GlobalObservabilityMiddleware>();
        return services;
    }
    private static IServiceCollection AddApiCorrelationMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<CorrelationIdMiddleware>();
        return services;
    }
    private static IServiceCollection AddApiExceptionHandlingMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ExceptionHandlingMiddleware>();
        return services;
    }
    private static IServiceCollection AddApiMaintenanceModeMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MaintenanceOptions>()
                .BindConfiguration(MaintenanceOptions.SectionName)
                .Validate(o => o.ExcludedPaths.All(PathMatcher.IsValidPath), "Maintenance:ExcludedPaths entries must start with '/' and be longer than '/'.")
                .ValidateOnStart();

        services.AddTransient<MaintenanceModeMiddleware>();

        return services;
    }
    private static IServiceCollection AddApiRequestLoggingMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RequestLoggingOptions>()
                        .BindConfiguration(RequestLoggingOptions.SectionName)
                        .Validate(o => o.QuietPaths.All(PathMatcher.IsValidPath), "RequestLogging:QuietPaths entries must start with '/' and be longer than '/'.")
                        .ValidateOnStart();

        services.AddTransient<RequestLoggingMiddleware>();

        return services;
    }
}