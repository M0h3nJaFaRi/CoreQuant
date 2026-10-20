using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Common.Presentation.Api.Configuration.Options;
using Common.Presentation.Api.Configuration.Validators;
using Common.Presentation.Api.Swagger;
using Common.Presentation.Contract;
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
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceNamespaceFallback, nameof(serviceNamespaceFallback));
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceVersionFallback, nameof(serviceVersionFallback));
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceComponentFallback, nameof(serviceComponentFallback));

        services.TryAddSingleton(sp =>
        {
            var serviceNamespace = configuration["ApplicationName"] ?? serviceNamespaceFallback;
            var serviceComponent = configuration["ApplicationComponent"] ?? serviceVersionFallback;
            var serviceVersion = assembly.GetName().Version?.ToString() ?? serviceComponentFallback;
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
    public static IServiceCollection AddCommonApi(this IServiceCollection services,
                                                       IConfiguration configuration)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddCommonApiSwagger(configuration);

        return services;
    }

    /// <summary>
    /// Binds <see cref="SwaggerOptions"/> and registers Swashbuckle when enabled.
    /// </summary>
    private static IServiceCollection AddCommonApiSwagger(this IServiceCollection services,
                                                               IConfiguration configuration,
                                                               Assembly? documentationAssembly = null)
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
}