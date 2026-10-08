using System.Reflection;
using Common.Api.Configuration.Options;
using Common.Api.Configuration.Validators;
using Microsoft.AspNetCore.Builder;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.Api.Swagger;

/// <summary>
/// OpenAPI / Swagger UI registration for HTTP hosts using <see cref="DependencyInjection.AddCommonApi"/>.
/// </summary>
public static class CommonApiSwaggerExtensions
{
    private const string DevUserSecurityId = "DevUser";
    private const string IngestionApiKeySecurityId = "IngestionApiKey";

    /// <summary>
    /// Binds <see cref="SwaggerOptions"/> and registers Swashbuckle when enabled.
    /// </summary>
    public static IServiceCollection AddCommonApiSwagger(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        Assembly? documentationAssembly = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        services.AddOptions<SwaggerOptions>()
            .Bind(configuration.GetSection(SwaggerOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IValidateOptions<SwaggerOptions>, SwaggerOptionsValidator>();

        var swaggerOptions = configuration.GetSection(SwaggerOptions.SectionName).Get<SwaggerOptions>() ?? new SwaggerOptions();
        if (!swaggerOptions.Enabled)
        {
            return services;
        }

        var xmlAssembly = documentationAssembly ?? Assembly.GetEntryAssembly();

        services.AddEndpointsApiExplorer();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>>(sp =>
            new VersionedSwaggerGenOptions(
                sp.GetRequiredService<IApiVersionDescriptionProvider>(),
                sp.GetRequiredService<IOptions<SwaggerOptions>>(),
                applicationName));
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".", StringComparison.Ordinal) ?? type.Name);

            if (swaggerOptions.IncludeDevUserHeader)
            {
                options.AddSecurityDefinition(
                    DevUserSecurityId,
                    new OpenApiSecurityScheme
                    {
                        Name = "X-User-Id",
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Description = "Development-only user id (snowflake). Requires Infrastructure:Acl:DevAuthEnabled.",
                    });
            }

            if (swaggerOptions.IncludeIngestionApiKey)
            {
                options.AddSecurityDefinition(
                    IngestionApiKeySecurityId,
                    new OpenApiSecurityScheme
                    {
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Description = "Collector ingestion API key (Ingestion:ApiKey).",
                    });
            }

            if (swaggerOptions.IncludeDevUserHeader)
            {
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = DevUserSecurityId,
                            },
                        },
                        Array.Empty<string>()
                    },
                });
            }

            if (swaggerOptions.IncludeIngestionApiKey)
            {
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = IngestionApiKeySecurityId,
                            },
                        },
                        Array.Empty<string>()
                    },
                });
            }

            if (xmlAssembly is not null)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{xmlAssembly.GetName().Name}.xml");
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }
        });

        return services;
    }

    /// <summary>
    /// Maps Swagger JSON and UI when <see cref="SwaggerOptions.Enabled"/> is true.
    /// </summary>
    public static WebApplication UseCommonApiSwagger(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var swaggerOptions = app.Services.GetRequiredService<IOptions<SwaggerOptions>>().Value;
        if (!swaggerOptions.Enabled)
        {
            return app;
        }

        if (app.Environment.IsProduction())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var versions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions;
            foreach (var description in versions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }

            options.RoutePrefix = swaggerOptions.RoutePrefix;
        });

        return app;
    }
}
