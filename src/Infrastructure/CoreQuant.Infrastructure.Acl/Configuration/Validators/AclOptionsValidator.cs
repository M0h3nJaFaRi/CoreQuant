using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Acl.Configuration.Options;

namespace CoreQuant.Infrastructure.Acl.Configuration.Validators;

/// <summary>
/// Validates <see cref="AclOptions"/> at options bind / host start.
/// </summary>
internal sealed class AclOptionsValidator : IValidateOptions<AclOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AclOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return ValidateOptionsResult.Success;
    }
}
