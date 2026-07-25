namespace ISkyPro.Contracts.PluginModels;

public sealed record PluginSdkV2InitializeRequest(
    IReadOnlyList<int> SupportedProtocolVersions,
    string InstanceId,
    string PluginId,
    PluginSdkV2DirectorySet Directories,
    string Token,
    string Encoding,
    PluginSdkV2RuntimeOptions? Runtime = null);

public sealed record PluginSdkV2InitializeResponse(
    int ProtocolVersion,
    string PluginId,
    string SdkName,
    string SdkVersion,
    IReadOnlyList<string> Capabilities,
    string Encoding);

public sealed record PluginSdkV2DirectorySet(
    string PluginDirectory,
    string PluginDataDirectory,
    string PluginConfigDirectory,
    string PluginCacheDirectory);

/// <summary>
/// Runtime limits negotiated during initialize. Older SDKs may ignore this
/// optional object, but conforming SDKs must honor the request timeout and
/// advertise whether they actually support concurrent event handling.
/// </summary>
public sealed record PluginSdkV2RuntimeOptions(
    int MaxConcurrentEvents = 1,
    int RequestTimeoutMilliseconds = PluginSdkV2Protocol.DefaultRequestTimeoutMilliseconds,
    int MaxHeaderLength = PluginSdkV2Protocol.MaxHeaderLength,
    int MaxPayloadLength = PluginSdkV2Protocol.MaxPayloadLength,
    int QueueCapacity = 64);

public static class PluginSdkV2HandshakeValidator
{
    public static IReadOnlyList<string> ValidateInitializeResponse(
        PluginSdkV2Manifest manifest,
        PluginSdkV2InitializeResponse response)
    {
        var errors = new List<string>();

        if (response.ProtocolVersion != PluginSdkV2Protocol.Version)
        {
            errors.Add($"initialize protocolVersion must be {PluginSdkV2Protocol.Version}.");
        }

        if (!string.Equals(response.PluginId, manifest.PluginId, StringComparison.Ordinal))
        {
            errors.Add("initialize pluginId must match manifest pluginId.");
        }

        if (string.IsNullOrWhiteSpace(response.SdkName))
        {
            errors.Add("initialize sdkName is required.");
        }

        if (string.IsNullOrWhiteSpace(response.SdkVersion))
        {
            errors.Add("initialize sdkVersion is required.");
        }

        if (!string.Equals(response.Encoding, PluginSdkV2Protocol.EncodingJson, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("initialize encoding must be json.");
        }

        return errors;
    }
}
