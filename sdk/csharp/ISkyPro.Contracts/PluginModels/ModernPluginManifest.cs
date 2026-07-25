namespace ISkyPro.Contracts.PluginModels;

public sealed record ModernPluginManifest(
    string PluginId,
    string Name,
    string Version,
    string Author,
    int ProtocolVersion,
    ModernPluginCapability Capabilities)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PluginId))
        {
            errors.Add("PluginId is required.");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            errors.Add("Version is required.");
        }

        if (ProtocolVersion is not ModernPluginProtocol.Version and not ModernPluginProtocol.Version2)
        {
            errors.Add($"ProtocolVersion must be {ModernPluginProtocol.Version} or {ModernPluginProtocol.Version2}.");
        }

        if (!Capabilities.HasFlag(ModernPluginCapability.ReceiveMessages))
        {
            errors.Add("ReceiveMessages capability is required.");
        }

        return errors;
    }
}
