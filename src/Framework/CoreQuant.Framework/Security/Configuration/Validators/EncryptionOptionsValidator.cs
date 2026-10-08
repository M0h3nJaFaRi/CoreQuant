using Framework.Security.Configuration.Options;
using Microsoft.Extensions.Options;

namespace Framework.Security.Configuration.Validators;

/// <summary>
/// Validates <see cref="EncryptionOptions"/> at options bind / host start.
/// Empty keys are allowed (encryption unused until secrets are supplied).
/// </summary>
internal sealed class EncryptionOptionsValidator : IValidateOptions<EncryptionOptions>
{
    private static readonly HashSet<int> AllowedAesKeyLengths = [16, 24, 32];

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, EncryptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();
        ValidateAes(options.AES, failures);
        ValidateRsa(options.RSA, failures);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateAes(AesEncryptionOptions aes, List<string> failures)
    {
        var keySet = !string.IsNullOrWhiteSpace(aes.Key);
        var ivSet = !string.IsNullOrWhiteSpace(aes.IV);

        if (!keySet && !ivSet)
        {
            return;
        }

        if (keySet != ivSet)
        {
            failures.Add($"{EncryptionOptions.SectionName}:AES requires both Key and IV when either is set.");
            return;
        }

        if (!TryDecodeBase64(aes.Key, out var keyBytes))
        {
            failures.Add($"{EncryptionOptions.SectionName}:AES:Key must be valid Base64.");
        }
        else if (!AllowedAesKeyLengths.Contains(keyBytes.Length))
        {
            failures.Add(
                $"{EncryptionOptions.SectionName}:AES:Key must decode to 16, 24, or 32 bytes (got {keyBytes.Length}).");
        }

        if (!TryDecodeBase64(aes.IV, out var ivBytes))
        {
            failures.Add($"{EncryptionOptions.SectionName}:AES:IV must be valid Base64.");
        }
        else if (ivBytes.Length != 16)
        {
            failures.Add(
                $"{EncryptionOptions.SectionName}:AES:IV must decode to 16 bytes (got {ivBytes.Length}).");
        }
    }

    private static void ValidateRsa(RsaEncryptionOptions rsa, List<string> failures)
    {
        if (rsa.HasPublicKey && !LooksLikePem(rsa.PublicKey))
        {
            failures.Add($"{EncryptionOptions.SectionName}:RSA:PublicKey must be a PEM-encoded key.");
        }

        if (rsa.HasPrivateKey && !LooksLikePem(rsa.PrivateKey))
        {
            failures.Add($"{EncryptionOptions.SectionName}:RSA:PrivateKey must be a PEM-encoded key.");
        }
    }

    private static bool TryDecodeBase64(string value, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private static bool LooksLikePem(string value)
        => value.Contains("BEGIN", StringComparison.Ordinal)
           && value.Contains("END", StringComparison.Ordinal);
}
