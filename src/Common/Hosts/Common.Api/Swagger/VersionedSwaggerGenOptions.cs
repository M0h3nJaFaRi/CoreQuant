using Asp.Versioning.ApiExplorer;
using Common.Api.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.Api.Swagger;

/// <summary>
/// Registers one OpenAPI document per discovered API version so URL versions stay distinct.
/// </summary>
internal sealed class VersionedSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _versions;
    private readonly SwaggerOptions _swagger;
    private readonly string _applicationName;

    public VersionedSwaggerGenOptions(
        IApiVersionDescriptionProvider versions,
        IOptions<SwaggerOptions> swagger,
        string applicationName)
    {
        _versions = versions;
        _swagger = swagger.Value;
        _applicationName = applicationName;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var title = string.IsNullOrWhiteSpace(_swagger.Title) ? _applicationName : _swagger.Title.Trim();

        foreach (var description in _versions.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = title,
                    Version = description.GroupName,
                    Description = _swagger.Description,
                });
        }
    }
}
