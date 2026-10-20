using Common.Presentation.Api.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Common.Presentation.Api.Configuration.Validators;

/// <summary>
/// Validates <see cref="SwaggerOptions"/> at host start.
/// </summary>
public sealed class SwaggerOptionsValidator : IValidateOptions<SwaggerOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SwaggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.DocumentName))
        {
            failures.Add($"{SwaggerOptions.SectionName}:DocumentName is required when Swagger is used.");
        }

        if (string.IsNullOrWhiteSpace(options.RoutePrefix))
        {
            failures.Add($"{SwaggerOptions.SectionName}:RoutePrefix is required when Swagger is used.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
