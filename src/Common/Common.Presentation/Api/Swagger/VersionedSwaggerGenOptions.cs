using Asp.Versioning.ApiExplorer;
using Common.Presentation.Api.Configuration.Options;
using Common.Presentation.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.Presentation.Api.Swagger;

/// <summary>
/// Registers one OpenAPI document per discovered API version so URL versions stay distinct.
/// </summary>
internal sealed class VersionedSwaggerGenOptions(IApiVersionDescriptionProvider versions, IOptions<SwaggerOptions> swagger, ServiceIdentity identity) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var title = string.IsNullOrWhiteSpace(swagger.Value.Title) ? identity.Name : swagger.Value.Title.Trim();

        foreach (var description in versions.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, new OpenApiInfo { Title = title, Version = description.GroupName, Description = swagger.Value.Description });
    }
}
