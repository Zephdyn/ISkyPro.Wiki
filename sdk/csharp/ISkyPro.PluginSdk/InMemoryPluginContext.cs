using ISkyPro.Contracts.BotModels;
using ISkyPro.Contracts.PluginModels;

namespace ISkyPro.PluginSdk;

public sealed class InMemoryPluginContext : IISkyProPluginContext
{
    private readonly List<PluginContextLogEntry> _logs = new();
    private readonly List<ModernPluginOutboundMessage> _outboundMessages = new();

    public InMemoryPluginContext(string pluginId)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }

    public IReadOnlyList<PluginContextLogEntry> Logs => _logs;

    public IReadOnlyList<ModernPluginOutboundMessage> OutboundMessages => _outboundMessages;

    public ValueTask WriteLogAsync(string level, string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logs.Add(new PluginContextLogEntry(DateTimeOffset.UtcNow, level, message));
        return ValueTask.CompletedTask;
    }

    public ValueTask SendTextAsync(
        BotMessageKind targetKind,
        string targetId,
        string content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _outboundMessages.Add(new ModernPluginOutboundMessage(targetKind, targetId, content));
        return ValueTask.CompletedTask;
    }
}
