using System.Text.Json;

namespace ISkyPro.Contracts.PluginModels;

public sealed record PluginSdkV2Manifest
{
    public string PluginId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int ProtocolVersion { get; init; } = PluginSdkV2Protocol.Version;

    public string SdkVersion { get; init; } = string.Empty;

    public PluginSdkV2TransportSpec Transport { get; init; } = new();

    public IReadOnlyList<PluginSdkV2PlatformSupport> SupportedPlatforms { get; init; } = Array.Empty<PluginSdkV2PlatformSupport>();

    public IReadOnlyList<PluginSdkV2EventSubscription> EventSubscriptions { get; init; } = Array.Empty<PluginSdkV2EventSubscription>();

    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();

    public IReadOnlyList<PluginSdkV2CommandSpec> Commands { get; init; } = Array.Empty<PluginSdkV2CommandSpec>();

    public IReadOnlyList<PluginSdkV2FilterSpec> Filters { get; init; } = Array.Empty<PluginSdkV2FilterSpec>();

    public PluginSdkV2ConcurrencyOptions Concurrency { get; init; } = new();

    public PluginSdkV2SettingsSpec? Settings { get; init; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        Require(PluginId, "pluginId", errors);
        Require(Name, "name", errors);
        Require(Version, "version", errors);
        Require(Author, "author", errors);
        Require(SdkVersion, "sdkVersion", errors);

        if (ProtocolVersion != PluginSdkV2Protocol.Version)
        {
            errors.Add($"protocolVersion must be {PluginSdkV2Protocol.Version}.");
        }

        ValidateTransport(errors);
        ValidateSupportedPlatforms(errors);
        ValidateEventSubscriptions(errors);
        ValidatePermissions(errors);
        ValidateCommands(errors);
        ValidateFilters(errors);
        ValidateConcurrency(errors);
        ValidateSettings(errors);
        return errors;
    }

    private void ValidateTransport(List<string> errors)
    {
        if (Transport is null)
        {
            errors.Add("transport is required.");
            return;
        }

        if (string.Equals(Transport.Type, PluginSdkV2Protocol.TransportStdioJsonRpc, StringComparison.OrdinalIgnoreCase))
        {
            if (Transport.Stdio is null)
            {
                errors.Add("transport.stdio is required for stdio-jsonrpc plugins.");
                return;
            }

            Require(Transport.Stdio.Command, "transport.stdio.command", errors);
            return;
        }

        if (string.Equals(Transport.Type, PluginSdkV2Protocol.TransportHttp, StringComparison.OrdinalIgnoreCase))
        {
            if (Transport.Http is null)
            {
                errors.Add("transport.http is required for HTTP plugins.");
                return;
            }

            if (!Uri.TryCreate(Transport.Http.BaseUrl, UriKind.Absolute, out var baseUri)
                || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            {
                errors.Add("transport.http.baseUrl must be an absolute HTTP or HTTPS URL.");
                return;
            }

            if (!Transport.Http.AllowRemote && !IsLoopback(baseUri))
            {
                errors.Add("remote HTTP plugins require transport.http.allowRemote to be true.");
            }

            Require(Transport.Http.Authentication, "transport.http.authentication", errors);
            return;
        }

        errors.Add($"transport.type must be '{PluginSdkV2Protocol.TransportStdioJsonRpc}' or '{PluginSdkV2Protocol.TransportHttp}'.");
    }

    private void ValidateSupportedPlatforms(List<string> errors)
    {
        if (SupportedPlatforms is null || SupportedPlatforms.Count == 0)
        {
            errors.Add("supportedPlatforms must contain at least one platform.");
            return;
        }

        foreach (var platform in SupportedPlatforms)
        {
            if (!IsKnownPlatform(platform.Platform))
            {
                errors.Add($"unsupported platform '{platform.Platform}'.");
            }
        }
    }

    private void ValidateEventSubscriptions(List<string> errors)
    {
        if (EventSubscriptions is null || EventSubscriptions.Count == 0)
        {
            errors.Add("eventSubscriptions must contain at least one subscription.");
            return;
        }

        foreach (var subscription in EventSubscriptions)
        {
            Require(subscription.EventType, "eventSubscriptions.eventType", errors);
        }
    }

    private void ValidatePermissions(List<string> errors)
    {
        foreach (var permission in Permissions ?? Array.Empty<string>())
        {
            Require(permission, "permissions", errors);
        }
    }

    private void ValidateCommands(List<string> errors)
    {
        foreach (var command in Commands ?? Array.Empty<PluginSdkV2CommandSpec>())
        {
            Require(command.Name, "commands.name", errors);
            if (command.Priority < 0)
            {
                errors.Add("commands.priority must not be negative.");
            }
        }
    }

    private void ValidateFilters(List<string> errors)
    {
        foreach (var filter in Filters ?? Array.Empty<PluginSdkV2FilterSpec>())
        {
            Require(filter.Name, "filters.name", errors);
            if (filter.TimeoutMilliseconds is < 50 or > 5000)
            {
                errors.Add("filters.timeoutMilliseconds must be between 50 and 5000.");
            }
        }
    }

    private void ValidateConcurrency(List<string> errors)
    {
        if (Concurrency is null)
        {
            errors.Add("concurrency is required.");
            return;
        }

        if (Concurrency.MaxConcurrentEvents <= 0)
        {
            errors.Add("concurrency.maxConcurrentEvents must be greater than 0.");
        }

        if (Concurrency.QueueCapacity <= 0)
        {
            errors.Add("concurrency.queueCapacity must be greater than 0.");
        }

        if (!string.Equals(Concurrency.OverflowPolicy, PluginSdkV2OverflowPolicies.RejectNew, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Concurrency.OverflowPolicy, PluginSdkV2OverflowPolicies.DropOldest, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Concurrency.OverflowPolicy, PluginSdkV2OverflowPolicies.WarnOnly, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("concurrency.overflowPolicy is invalid.");
        }
    }

    private void ValidateSettings(List<string> errors)
    {
        if (Settings is null)
        {
            return;
        }

        if (Settings.PageUrl is not null
            && (!Uri.TryCreate(Settings.PageUrl, UriKind.Absolute, out var settingsUri)
                || settingsUri.Scheme != Uri.UriSchemeHttp
                || !IsLoopback(settingsUri)))
        {
            errors.Add("settings.pageUrl must be an absolute loopback HTTP URL.");
        }

        foreach (var item in Settings.ConfigSchema ?? new Dictionary<string, PluginSdkV2ConfigFieldSpec>())
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                errors.Add("settings.configSchema keys must not be empty.");
                continue;
            }

            var field = item.Value;
            if (field is null)
            {
                errors.Add($"settings.configSchema.{item.Key} is required.");
                continue;
            }

            var type = NormalizeSettingsFieldType(field.Type);
            if (!IsKnownSettingsFieldType(type))
            {
                errors.Add($"settings.configSchema.{item.Key}.type is invalid.");
                continue;
            }

            if (string.Equals(type, "select", StringComparison.Ordinal)
                && (field.Options is null || field.Options.Count == 0 || field.Options.Any(string.IsNullOrWhiteSpace)))
            {
                errors.Add($"settings.configSchema.{item.Key}.options must contain at least one non-empty value.");
            }

            if (field.DefaultValue is { } defaultValue
                && defaultValue.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null
                && !IsSettingsDefaultValueValid(type, field, defaultValue))
            {
                errors.Add($"settings.configSchema.{item.Key}.defaultValue does not match the field type.");
            }
        }
    }

    private static void Require(string? value, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
        }
    }

    private static bool IsKnownPlatform(string platform)
    {
        return string.Equals(platform, "windows", StringComparison.OrdinalIgnoreCase)
            || string.Equals(platform, "linux", StringComparison.OrdinalIgnoreCase)
            || string.Equals(platform, "osx", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLoopback(Uri uri)
    {
        return uri.IsLoopback
            || string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Host, "127.0.0.1", StringComparison.Ordinal)
            || string.Equals(uri.Host, "::1", StringComparison.Ordinal);
    }

    private static string NormalizeSettingsFieldType(string? type)
    {
        return string.IsNullOrWhiteSpace(type)
            ? "string"
            : type.Trim().ToLowerInvariant();
    }

    private static bool IsKnownSettingsFieldType(string type)
    {
        return type is "string" or "number" or "boolean" or "select" or "path" or "secret";
    }

    private static bool IsSettingsDefaultValueValid(
        string type,
        PluginSdkV2ConfigFieldSpec field,
        JsonElement value)
    {
        return type switch
        {
            "string" or "path" or "secret" => value.ValueKind == JsonValueKind.String,
            "number" => value.ValueKind == JsonValueKind.Number,
            "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "select" => value.ValueKind == JsonValueKind.String
                && (field.Options ?? Array.Empty<string>()).Contains(value.GetString(), StringComparer.Ordinal),
            _ => false
        };
    }
}

public sealed record PluginSdkV2TransportSpec
{
    public string Type { get; init; } = PluginSdkV2Protocol.TransportStdioJsonRpc;

    public PluginSdkV2StdioProcessSpec? Stdio { get; init; }

    public PluginSdkV2HttpTransportSpec? Http { get; init; }
}

public sealed record PluginSdkV2StdioProcessSpec
{
    public string Command { get; init; } = string.Empty;

    public IReadOnlyList<string> Args { get; init; } = Array.Empty<string>();

    public string? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, string> Environment { get; init; } = new Dictionary<string, string>();
}

public sealed record PluginSdkV2HttpTransportSpec
{
    public string BaseUrl { get; init; } = string.Empty;

    public bool AllowRemote { get; init; }

    public string Authentication { get; init; } = "token";
}

public sealed record PluginSdkV2PlatformSupport
{
    public string Platform { get; init; } = string.Empty;

    public IReadOnlyList<string> Architectures { get; init; } = Array.Empty<string>();
}

public sealed record PluginSdkV2EventSubscription
{
    public string EventType { get; init; } = string.Empty;

    public IReadOnlyList<string> BotAccountIds { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ConversationIds { get; init; } = Array.Empty<string>();
}

public sealed record PluginSdkV2CommandSpec
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> Aliases { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Prefixes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ConversationScopes { get; init; } = Array.Empty<string>();

    public int Priority { get; init; }
}

public sealed record PluginSdkV2FilterSpec
{
    public string Name { get; init; } = string.Empty;

    public int TimeoutMilliseconds { get; init; } = 500;

    public string FailurePolicy { get; init; } = "allow";
}

public sealed record PluginSdkV2ConcurrencyOptions
{
    public int MaxConcurrentEvents { get; init; } = 1;

    public int QueueCapacity { get; init; } = 64;

    public bool AllowSameConversationConcurrency { get; init; }

    public string OverflowPolicy { get; init; } = PluginSdkV2OverflowPolicies.RejectNew;
}

public sealed record PluginSdkV2SettingsSpec
{
    public IReadOnlyDictionary<string, PluginSdkV2ConfigFieldSpec> ConfigSchema { get; init; } =
        new Dictionary<string, PluginSdkV2ConfigFieldSpec>();

    public string? PageUrl { get; init; }
}

public sealed record PluginSdkV2ConfigFieldSpec
{
    public string Type { get; init; } = "string";

    public string Label { get; init; } = string.Empty;

    public bool Secret { get; init; }

    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();

    public JsonElement? DefaultValue { get; init; }
}
