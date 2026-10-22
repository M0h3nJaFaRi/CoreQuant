using System.Buffers;
using Microsoft.Extensions.Options;
using Common.Presentation.Api.Extensions;
using Common.Presentation.Api.Configuration.Options;

namespace Common.Presentation.Api.Configuration.Validators;
public sealed class ApiKeyOptionsValidator : IValidateOptions<ApiKeyOptions>
{
    private const int MinKeyLength = 16;
    public ValidateOptionsResult Validate(string? name, ApiKeyOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.HeaderName))
            errors.Add($"{ApiKeyOptions.SectionName}:HeaderName is required.");

        foreach (var path in options.ExcludedPaths.Where(p => !PathMatcher.IsValidPath(p)))
            errors.Add($"{ApiKeyOptions.SectionName}:ExcludedPaths entry '{path}' must start with '/' and be longer than '/'.");

        if (!options.Clients.Values.Any(c => c is { Enabled: true }))
            errors.Add($"{ApiKeyOptions.SectionName}:Clients must contain at least one enabled client.");

        var seenKeys = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (clientName, client) in options.Clients)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                errors.Add("A client has an empty name.");
                continue;
            }

            if (client is null || !client.Enabled)
                continue;

            var keys = client.Keys.Where(k => !string.IsNullOrWhiteSpace(k))
                                  .Select(k => k.Trim())
                                  .ToArray();

            if (keys.Length == 0)
                errors.Add($"Client '{clientName}' is enabled but has no non-empty keys.");

            foreach (var key in keys)
            {
                if (key.Length < MinKeyLength)
                    errors.Add($"Client '{clientName}' has a key shorter than {MinKeyLength} characters.");

                if (!seenKeys.TryAdd(key, clientName))
                    errors.Add($"Clients '{seenKeys[key]}' and '{clientName}' share the same key.");
            }
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}