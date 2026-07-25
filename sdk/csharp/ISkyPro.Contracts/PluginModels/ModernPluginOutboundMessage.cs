using ISkyPro.Contracts.BotModels;

namespace ISkyPro.Contracts.PluginModels;

public sealed record ModernPluginOutboundMessage(
    BotMessageKind TargetKind,
    string TargetId,
    string Content);
