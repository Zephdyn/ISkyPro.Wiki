namespace ISkyPro.Contracts.BotModels;

public sealed record BotAccountContext(
    string BotAccountId,
    string Platform,
    string ExternalBotId,
    string DisplayName)
{
    public static BotAccountContext Unknown { get; } = new(
        "unknown",
        "unknown",
        string.Empty,
        "Unknown Bot");
}
