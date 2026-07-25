using ISkyPro.Contracts.PluginModels;
using System.Text.Json;

namespace ISkyPro.PluginSdk.V2;

public sealed class InMemoryPluginV2Context : IISkyProPluginV2Context
{
    private readonly List<PluginV2SdkCall> _sdkCalls = new();
    private readonly List<PluginV2LogEntry> _logs = new();
    private readonly Dictionary<string, Queue<JsonElement>> _results = new(StringComparer.Ordinal);

    public InMemoryPluginV2Context(string pluginId)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }

    public IReadOnlyList<PluginV2SdkCall> SdkCalls => _sdkCalls;

    public IReadOnlyList<PluginV2LogEntry> Logs => _logs;

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

    public ValueTask ReplyTextAsync(
        PluginSdkV2MessageReference messageReference,
        string content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Reply content is required.", nameof(content));
        }

        _sdkCalls.Add(new PluginV2SdkCall(
            "messages.replyText",
            new Dictionary<string, object?>
            {
                ["messageReference"] = messageReference,
                ["content"] = content
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
