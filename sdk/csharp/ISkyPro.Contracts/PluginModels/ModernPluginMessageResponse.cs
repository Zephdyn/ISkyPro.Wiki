namespace ISkyPro.Contracts.PluginModels;

public sealed record ModernPluginMessageResponse(
    bool Accepted,
    bool Intercepted,
    IReadOnlyList<ModernPluginOutboundMessage> OutboundMessages,
    string? Error)
{
    public static ModernPluginMessageResponse Handled(
        bool intercepted = false,
        IReadOnlyList<ModernPluginOutboundMessage>? outboundMessages = null)
    {
        return new ModernPluginMessageResponse(
            Accepted: true,
            Intercepted: intercepted,
            OutboundMessages: outboundMessages ?? Array.Empty<ModernPluginOutboundMessage>(),
            Error: null);
    }

    public static ModernPluginMessageResponse Failed(string error)
    {
        return new ModernPluginMessageResponse(
            Accepted: false,
            Intercepted: false,
            OutboundMessages: Array.Empty<ModernPluginOutboundMessage>(),
            Error: error);
    }
}
