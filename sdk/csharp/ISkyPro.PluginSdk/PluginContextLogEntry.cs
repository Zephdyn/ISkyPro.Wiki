namespace ISkyPro.PluginSdk;

public sealed record PluginContextLogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Message);
