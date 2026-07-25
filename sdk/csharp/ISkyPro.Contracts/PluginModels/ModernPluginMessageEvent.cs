using ISkyPro.Contracts.BotModels;

namespace ISkyPro.Contracts.PluginModels;

public sealed record ModernPluginMessageEvent(
    string EventId,
    DateTimeOffset Timestamp,
    BotAccountContext Bot,
    BotMessageEnvelope Message)
{
    public ModernPluginMessageEvent(
        string EventId,
        DateTimeOffset Timestamp,
        BotMessageEnvelope Message)
        : this(EventId, Timestamp, BotAccountContext.Unknown, Message)
    {
    }

    public static ModernPluginMessageEvent FromMessage(BotMessageEnvelope message)
    {
        return FromMessage(message, BotAccountContext.Unknown);
    }

    public static ModernPluginMessageEvent FromMessage(
        BotMessageEnvelope message,
        BotAccountContext bot)
    {
        return new ModernPluginMessageEvent(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            bot,
            message);
    }
}
