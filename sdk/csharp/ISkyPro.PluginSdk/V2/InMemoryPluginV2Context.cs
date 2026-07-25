using ISkyPro.Contracts.PluginModels;
using System.Text.Json;

namespace ISkyPro.PluginSdk.V2;

public sealed class InMemoryPluginV2Context : IISkyProPluginV2Context, IPluginV2MessageTransport
{
    private readonly List<PluginV2SdkCall> _sdkCalls = new();
    private readonly List<PluginV2LogEntry> _logs = new();
    private readonly List<PluginV2OutgoingMessage> _outgoingMessages = new();
    private readonly Dictionary<string, Queue<JsonElement>> _results = new(StringComparer.Ordinal);

    public InMemoryPluginV2Context(string pluginId)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }

    public IMessageService Messages => new MessageService(this);

    public IReadOnlyList<PluginV2SdkCall> SdkCalls => _sdkCalls;

    public IReadOnlyList<PluginV2LogEntry> Logs => _logs;

    public IReadOnlyList<PluginV2OutgoingMessage> OutgoingMessages => _outgoingMessages;

    public MessageContext CreateMessageContext(
        PluginSdkV2EventEnvelope pluginEvent,
        CancellationToken cancellationToken = default)
    {
        return new MessageContext(pluginEvent, this, cancellationToken);
    }

    public void EnqueueResult(string method, object? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        if (!_results.TryGetValue(method, out var results))
        {
            results = new Queue<JsonElement>();
            _results[method] = results;
        }

        results.Enqueue(JsonSerializer.SerializeToElement(result));
    }

    ValueTask IPluginV2MessageTransport.ReplyAsync(
        PluginSdkV2MessageReference reference,
        OutgoingMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(message);
        _outgoingMessages.Add(new PluginV2OutgoingMessage(reference, null, message));
        _sdkCalls.Add(new PluginV2SdkCall(
            PluginSdkV2Protocol.MessagesReplyMethod,
            new Dictionary<string, object?>
            {
                ["reference"] = reference,
                ["message"] = message
            }));
        return ValueTask.CompletedTask;
    }

    ValueTask IPluginV2MessageTransport.SendAsync(
        MessageTarget target,
        OutgoingMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(message);
        _outgoingMessages.Add(new PluginV2OutgoingMessage(null, target, message));
        _sdkCalls.Add(new PluginV2SdkCall(
            PluginSdkV2Protocol.MessagesSendMethod,
            new Dictionary<string, object?>
            {
                ["target"] = target,
                ["message"] = message
            }));
        return ValueTask.CompletedTask;
    }

    public ValueTask InvokeAsync(
        string method,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException("SDK method is required.", nameof(method));
        }

        RecordCall(method, parameters);
        return ValueTask.CompletedTask;
    }

    public ValueTask<JsonElement> InvokeWithResultAsync(
        string method,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordCall(method, parameters);
        if (_results.TryGetValue(method, out var results) && results.Count > 0)
        {
            return ValueTask.FromResult(results.Dequeue());
        }

        throw new InvalidOperationException(
            $"No in-memory SDK result was configured for method '{method}'.");
    }

    public ValueTask WriteLogAsync(
        string level,
        string message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logs.Add(new PluginV2LogEntry(DateTimeOffset.UtcNow, level, message));
        return ValueTask.CompletedTask;
    }

    private void RecordCall(
        string method,
        IReadOnlyDictionary<string, object?> parameters)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException("SDK method is required.", nameof(method));
        }

        _sdkCalls.Add(new PluginV2SdkCall(method, new Dictionary<string, object?>(parameters)));
    }
}

public sealed record PluginV2SdkCall(
    string Method,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record PluginV2LogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Message);

public sealed record PluginV2OutgoingMessage(
    PluginSdkV2MessageReference? Reference,
    MessageTarget? Target,
    OutgoingMessage Message);
