using System.Collections.Concurrent;
using System.Text.Json;
using ISkyPro.Contracts.PluginModels;

namespace ISkyPro.PluginSdk.V2;

/// <summary>
/// Options for the official C# Plugin SDK v2 stdio runtime.
/// </summary>
public sealed class StdioPluginV2HostOptions
{
    /// <summary>
    /// Optional local cap for concurrent event handlers. The effective value
    /// never exceeds the value supplied by Main during initialize.
    /// </summary>
    public int? MaxConcurrentEvents { get; init; }

    /// <summary>
    /// Optional local cap for queued event requests. The effective value never
    /// exceeds the value supplied by Main during initialize.
    /// </summary>
    public int? QueueCapacity { get; init; }

    /// <summary>
    /// Fallback SDK API request timeout used when Main does not provide one.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromMilliseconds(PluginSdkV2Protocol.DefaultRequestTimeoutMilliseconds);

    public Stream? Input { get; init; }

    public Stream? Output { get; init; }

    /// <summary>
    /// Optional manifest override. By default the host reads manifest.json from
    /// the plugin working directory so identity and permissions have one source.
    /// </summary>
    public PluginSdkV2Manifest? Manifest { get; init; }

    public string? ManifestPath { get; init; }
}

/// <summary>
/// Runs a C# Plugin SDK v2 implementation over the standard stdio-jsonrpc
/// transport. The reader loop remains independent from event handlers so
/// plugin-to-Main SDK requests and concurrent events can be multiplexed safely.
/// </summary>
public static class StdioPluginV2Host
{
    public static Task RunAsync(
        IISkyProPluginV2 plugin,
        CancellationToken cancellationToken = default)
    {
        return RunAsync(plugin, new StdioPluginV2HostOptions(), cancellationToken);
    }

    public static async Task RunAsync(
        IISkyProPluginV2 plugin,
        StdioPluginV2HostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(options);

        var manifest = options.Manifest ?? LoadManifest(options.ManifestPath);
        var manifestErrors = manifest.Validate();
        if (manifestErrors.Count > 0)
        {
            throw new InvalidOperationException(
                "Invalid Plugin SDK v2 manifest: " + string.Join("; ", manifestErrors));
        }

        var input = options.Input ?? Console.OpenStandardInput();
        var output = options.Output ?? Console.OpenStandardOutput();
        var connection = new HostConnection(plugin, manifest, options, input, output);
        await connection.RunAsync(cancellationToken);
    }

    private static PluginSdkV2Manifest LoadManifest(string? configuredPath)
    {
        var manifestPath = string.IsNullOrWhiteSpace(configuredPath)
            ? ResolveDefaultManifestPath()
            : Path.GetFullPath(configuredPath);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException(
                "Plugin SDK v2 manifest.json was not found. Set StdioPluginV2HostOptions.ManifestPath when using a non-standard layout.",
                manifestPath);
        }

        return JsonSerializer.Deserialize<PluginSdkV2Manifest>(
                File.ReadAllText(manifestPath),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Plugin SDK v2 manifest.json is empty.");
    }

    private static string ResolveDefaultManifestPath()
    {
        var workingDirectoryPath = Path.Combine(Environment.CurrentDirectory, "manifest.json");
        if (File.Exists(workingDirectoryPath))
        {
            return workingDirectoryPath;
        }

        return Path.Combine(AppContext.BaseDirectory, "manifest.json");
    }

    private sealed class HostConnection
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IISkyProPluginV2 _plugin;
        private readonly PluginSdkV2Manifest _manifest;
        private readonly StdioPluginV2HostOptions _options;
        private readonly Stream _input;
        private readonly Stream _output;
        private readonly SemaphoreSlim _writeGate = new(1, 1);
        private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pending = new();
        private readonly object _eventTasksGate = new();
        private readonly List<Task> _eventTasks = new();
        private readonly CancellationTokenSource _stopCts = new();

        private SemaphoreSlim? _eventGate;
        private SemaphoreSlim? _eventSlots;
        private CancellationToken _runCancellationToken;
        private string _token = string.Empty;
        private TimeSpan _requestTimeout;
        private long _nextRequestId = 1000;
        private int _initialized;
        private int _stopping;

        public HostConnection(
            IISkyProPluginV2 plugin,
            PluginSdkV2Manifest manifest,
            StdioPluginV2HostOptions options,
            Stream input,
            Stream output)
        {
            _plugin = plugin;
            _manifest = manifest;
            _options = options;
            _input = input;
            _output = output;
            _requestTimeout = NormalizeTimeout(options.RequestTimeout);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _stopCts.Token);
            _runCancellationToken = linkedCts.Token;

            try
            {
                while (!linkedCts.IsCancellationRequested)
                {
                    using var document = await StdioJsonRpcFraming.ReadAsync(_input, linkedCts.Token);
                    if (document is null)
                    {
                        break;
                    }

                    HandleMessage(document.RootElement);
                }
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
            }
            finally
            {
                FailPending(new EndOfStreamException("ISkyPro stdio-jsonrpc connection closed."));
                await AwaitEventTasksAsync();
                _eventGate?.Dispose();
                _eventSlots?.Dispose();
                _writeGate.Dispose();
                _stopCts.Dispose();
            }
        }

        private void HandleMessage(JsonElement root)
        {
            if (root.TryGetProperty("method", out var methodElement))
            {
                var method = methodElement.GetString();
                if (string.IsNullOrWhiteSpace(method))
                {
                    throw new InvalidDataException("JSON-RPC method is required.");
                }

                var requestId = root.TryGetProperty("id", out var idElement)
                    ? idElement.Clone()
                    : (JsonElement?)null;
                var parameters = root.TryGetProperty("params", out var paramsElement)
                    ? paramsElement.Clone()
                    : JsonSerializer.SerializeToElement(new Dictionary<string, object?>(), JsonOptions);
                _ = HandleRequestAsync(requestId, method, parameters);
                return;
            }

            if (root.TryGetProperty("id", out var responseId)
                && (root.TryGetProperty("result", out var result)
                    || root.TryGetProperty("error", out var error)))
            {
                var id = ReadRequestId(responseId);
                if (!_pending.TryGetValue(id, out var completion))
                {
                    return;
                }

                if (root.TryGetProperty("error", out error))
                {
                    completion.TrySetException(new PluginSdkV2RpcException(
                        ReadErrorCode(error),
                        ReadErrorMessage(error),
                        ReadErrorData(error)));
                }
                else
                {
                    completion.TrySetResult(result.Clone());
                }

                return;
            }

            throw new InvalidDataException("Invalid JSON-RPC message envelope.");
        }

        private async Task HandleRequestAsync(
            JsonElement? requestId,
            string method,
            JsonElement parameters)
        {
            try
            {
                switch (method)
                {
                    case PluginSdkV2Protocol.InitializeMethod:
                        await HandleInitializeAsync(requestId, parameters);
                        return;
                    case PluginSdkV2Protocol.MessageEventMethod:
                        ScheduleEvent(requestId, parameters);
                        return;
                    case PluginSdkV2Protocol.StopMethod:
                    case PluginSdkV2Protocol.ShutdownMethod:
                        await HandleStopAsync(requestId);
                        return;
                    default:
                        if (requestId is { } unknownRequestId)
                        {
                            await WriteErrorResponseAsync(
                                unknownRequestId,
                                PluginSdkV2Protocol.MethodNotFound,
                                $"Unknown JSON-RPC method: {method}",
                                _runCancellationToken);
                        }

                        return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (requestId is { } failedRequestId)
                {
                    await WriteErrorResponseAsync(
                        failedRequestId,
                        ex is ArgumentException or InvalidDataException
                            ? PluginSdkV2Protocol.InvalidParams
                            : PluginSdkV2Protocol.PluginError,
                        ex.Message,
                        _runCancellationToken);
                }
            }
        }

        private async Task HandleInitializeAsync(JsonElement? requestId, JsonElement parameters)
        {
            if (requestId is null)
            {
                throw new InvalidDataException("iskypro.initialize must be a JSON-RPC request.");
            }

            if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            {
                throw new InvalidOperationException("Plugin SDK v2 is already initialized.");
            }

            var request = JsonSerializer.Deserialize<PluginSdkV2InitializeRequest>(parameters, JsonOptions)
                ?? throw new InvalidDataException("Initialize request is empty.");
            if (!request.SupportedProtocolVersions.Contains(PluginSdkV2Protocol.Version))
            {
                throw new InvalidDataException("Main does not support Plugin SDK protocol version 2.");
            }

            if (!string.Equals(request.PluginId, _manifest.PluginId, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Initialize pluginId does not match the plugin manifest.");
            }

            if (!string.Equals(request.Encoding, PluginSdkV2Protocol.EncodingJson, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Plugin SDK v2 only supports json encoding.");
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                throw new InvalidDataException("Initialize token is required.");
            }

            _token = request.Token;
            var runtime = request.Runtime ?? new PluginSdkV2RuntimeOptions();
            var mainLimit = Math.Max(1, runtime.MaxConcurrentEvents);
            var localLimit = _options.MaxConcurrentEvents is > 0
                ? _options.MaxConcurrentEvents.Value
                : mainLimit;
            var effectiveConcurrency = Math.Min(mainLimit, localLimit);
            _eventGate = new SemaphoreSlim(effectiveConcurrency, effectiveConcurrency);
            var mainQueueCapacity = Math.Max(1, runtime.QueueCapacity);
            var localQueueCapacity = _options.QueueCapacity is > 0
                ? _options.QueueCapacity.Value
                : mainQueueCapacity;
            var effectiveQueueCapacity = Math.Min(mainQueueCapacity, localQueueCapacity);
            _eventSlots = new SemaphoreSlim(
                effectiveConcurrency + effectiveQueueCapacity,
                effectiveConcurrency + effectiveQueueCapacity);
            _requestTimeout = NormalizeTimeout(TimeSpan.FromMilliseconds(
                runtime.RequestTimeoutMilliseconds > 0
                    ? runtime.RequestTimeoutMilliseconds
                    : _options.RequestTimeout.TotalMilliseconds));

            var response = new PluginSdkV2InitializeResponse(
                PluginSdkV2Protocol.Version,
                _manifest.PluginId,
                "iskypro-csharp-sdk-v2",
                PluginV2SdkMethods.SdkVersion,
                new[]
                {
                    PluginSdkV2Protocol.MessageEventMethod,
                    PluginSdkV2Protocol.LogWriteMethod,
                    PluginSdkV2Protocol.MessagesReplyMethod,
                    PluginSdkV2Protocol.MessagesSendMethod,
                    PluginSdkV2Protocol.CapabilityBidirectionalRequests,
                    PluginSdkV2Protocol.CapabilityConcurrentEvents,
                    PluginSdkV2Protocol.CapabilityGracefulShutdown
                },
                PluginSdkV2Protocol.EncodingJson);
            await WriteSuccessResponseAsync(requestId.Value, response, _runCancellationToken);
        }

        private void ScheduleEvent(JsonElement? requestId, JsonElement parameters)
        {
            if (Volatile.Read(ref _initialized) == 0)
            {
                _ = requestId is { } id
                    ? WriteErrorResponseAsync(
                        id,
                        PluginSdkV2Protocol.InvalidRequest,
                        "Plugin must be initialized before events are dispatched.",
                        _runCancellationToken)
                    : Task.CompletedTask;
                return;
            }

            if (Volatile.Read(ref _stopping) != 0)
            {
                _ = requestId is { } id
                    ? WriteErrorResponseAsync(
                        id,
                        PluginSdkV2Protocol.PluginError,
                        "Plugin is stopping.",
                        _runCancellationToken)
                    : Task.CompletedTask;
                return;
            }

            var eventSlots = _eventSlots
                ?? throw new InvalidOperationException("Plugin event queue was not initialized.");
            if (!eventSlots.Wait(0))
            {
                if (requestId is { } overflowRequestId)
                {
                    _ = WriteSuccessResponseAsync(
                        overflowRequestId,
                        new PluginSdkV2EventAck(
                            ReadEventId(parameters),
                            Accepted: false,
                            Error: "Plugin SDK v2 local event queue is full."),
                        _runCancellationToken);
                }

                return;
            }

            var task = HandleEventAsync(requestId, parameters);
            lock (_eventTasksGate)
            {
                _eventTasks.Add(task);
            }

            _ = task.ContinueWith(
                completed =>
                {
                    lock (_eventTasksGate)
                    {
                        _eventTasks.Remove(completed);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private async Task HandleEventAsync(JsonElement? requestId, JsonElement parameters)
        {
            var eventGate = _eventGate
                ?? throw new InvalidOperationException("Plugin event gate was not initialized.");
            var eventGateHeld = false;
            try
            {
                await eventGate.WaitAsync(_runCancellationToken);
                eventGateHeld = true;
                var pluginEvent = JsonSerializer.Deserialize<PluginSdkV2EventEnvelope>(parameters, JsonOptions)
                    ?? throw new InvalidDataException("Plugin event payload is empty.");
                var context = new StdioPluginContext(this, _manifest.PluginId);
                var message = new MessageContext(pluginEvent, context, _runCancellationToken);
                PluginSdkV2EventAck ack;
                try
                {
                    ack = await _plugin.OnMessageAsync(message, context, _runCancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    ack = new PluginSdkV2EventAck(pluginEvent.EventId, Accepted: false, Error: ex.Message);
                }

                if (!string.Equals(ack.EventId, pluginEvent.EventId, StringComparison.Ordinal))
                {
                    ack = ack with
                    {
                        EventId = pluginEvent.EventId,
                        Accepted = false,
                        Error = "Plugin returned an ACK with the wrong eventId."
                    };
                }

                if (requestId is { } id)
                {
                    await WriteSuccessResponseAsync(id, ack, _runCancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (requestId is { } id)
                {
                    await WriteErrorResponseAsync(
                        id,
                        PluginSdkV2Protocol.PluginError,
                        ex.Message,
                        _runCancellationToken);
                }
            }
            finally
            {
                if (eventGateHeld)
                {
                    eventGate.Release();
                }

                _eventSlots?.Release();
            }
        }

        private static string ReadEventId(JsonElement parameters)
        {
            return parameters.ValueKind == JsonValueKind.Object
                && parameters.TryGetProperty("eventId", out var eventId)
                && eventId.ValueKind == JsonValueKind.String
                ? eventId.GetString() ?? string.Empty
                : string.Empty;
        }

        private async Task HandleStopAsync(JsonElement? requestId)
        {
            if (Interlocked.Exchange(ref _stopping, 1) != 0)
            {
                return;
            }

            if (requestId is { } id)
            {
                await WriteSuccessResponseAsync(
                    id,
                    new { accepted = true },
                    _runCancellationToken);
            }

            Task[] activeTasks;
            lock (_eventTasksGate)
            {
                activeTasks = _eventTasks.ToArray();
            }

            if (activeTasks.Length == 0)
            {
                _stopCts.Cancel();
                return;
            }

            _ = Task.WhenAll(activeTasks).ContinueWith(
                _ => _stopCts.Cancel(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public async ValueTask<JsonElement> InvokeAsync(
            string method,
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentException("SDK method is required.", nameof(method));
            }

            if (Volatile.Read(ref _initialized) == 0 || string.IsNullOrWhiteSpace(_token))
            {
                throw new InvalidOperationException("Plugin SDK v2 is not initialized.");
            }

            var id = Interlocked.Increment(ref _nextRequestId);
            var completion = new TaskCompletionSource<JsonElement>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_pending.TryAdd(id, completion))
            {
                throw new InvalidOperationException($"Duplicate JSON-RPC request id: {id}.");
            }

            var requestParameters = new Dictionary<string, object?>(parameters, StringComparer.Ordinal)
            {
                ["token"] = _token
            };

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _runCancellationToken);
            linkedCts.CancelAfter(_requestTimeout);
            using var registration = linkedCts.Token.Register(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    completion.TrySetCanceled(cancellationToken);
                }
                else if (_runCancellationToken.IsCancellationRequested)
                {
                    completion.TrySetCanceled(_runCancellationToken);
                }
                else
                {
                    completion.TrySetException(new TimeoutException(
                        $"SDK request '{method}' timed out after {_requestTimeout.TotalMilliseconds:0} ms."));
                }
            });

            try
            {
                await WriteRequestAsync(id, method, requestParameters, linkedCts.Token);
                return await completion.Task;
            }
            finally
            {
                _pending.TryRemove(id, out _);
            }
        }

        private async Task WriteRequestAsync(
            long id,
            string method,
            object parameters,
            CancellationToken cancellationToken)
        {
            await WritePayloadAsync(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("jsonrpc", PluginSdkV2Protocol.JsonRpcVersion);
                writer.WriteNumber("id", id);
                writer.WriteString("method", method);
                writer.WritePropertyName("params");
                JsonSerializer.Serialize(writer, parameters, JsonOptions);
                writer.WriteEndObject();
            }, cancellationToken);
        }

        private async Task WriteSuccessResponseAsync(
            JsonElement id,
            object? result,
            CancellationToken cancellationToken)
        {
            await WritePayloadAsync(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("jsonrpc", PluginSdkV2Protocol.JsonRpcVersion);
                writer.WritePropertyName("id");
                id.WriteTo(writer);
                writer.WritePropertyName("result");
                JsonSerializer.Serialize(writer, result, JsonOptions);
                writer.WriteEndObject();
            }, cancellationToken);
        }

        private async Task WriteErrorResponseAsync(
            JsonElement id,
            int code,
            string message,
            CancellationToken cancellationToken)
        {
            await WritePayloadAsync(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("jsonrpc", PluginSdkV2Protocol.JsonRpcVersion);
                writer.WritePropertyName("id");
                id.WriteTo(writer);
                writer.WritePropertyName("error");
                writer.WriteStartObject();
                writer.WriteNumber("code", code);
                writer.WriteString("message", message);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }, cancellationToken);
        }

        private async Task WritePayloadAsync(
            Action<Utf8JsonWriter> write,
            CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                write(writer);
            }

            await _writeGate.WaitAsync(cancellationToken);
            try
            {
                await StdioJsonRpcFraming.WriteRawAsync(
                    _output,
                    stream.ToArray(),
                    cancellationToken);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        private async Task AwaitEventTasksAsync()
        {
            Task[] tasks;
            lock (_eventTasksGate)
            {
                tasks = _eventTasks.ToArray();
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void FailPending(Exception exception)
        {
            foreach (var completion in _pending.Values)
            {
                completion.TrySetException(exception);
            }
        }

        private static long ReadRequestId(JsonElement id)
        {
            return id.ValueKind switch
            {
                JsonValueKind.Number when id.TryGetInt64(out var numeric) => numeric,
                JsonValueKind.String when long.TryParse(id.GetString(), out var numeric) => numeric,
                _ => throw new InvalidDataException("JSON-RPC response id is invalid.")
            };
        }

        private static int ReadErrorCode(JsonElement error)
        {
            return error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("code", out var code)
                && code.TryGetInt32(out var value)
                ? value
                : PluginSdkV2Protocol.PluginError;
        }

        private static string ReadErrorMessage(JsonElement error)
        {
            return error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                ? message.GetString() ?? "JSON-RPC error."
                : error.GetRawText();
        }

        private static JsonElement? ReadErrorData(JsonElement error)
        {
            return error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("data", out var data)
                ? data.Clone()
                : null;
        }

        private static TimeSpan NormalizeTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
            {
                return TimeSpan.FromMilliseconds(PluginSdkV2Protocol.DefaultRequestTimeoutMilliseconds);
            }

            return timeout > TimeSpan.FromMinutes(5)
                ? TimeSpan.FromMinutes(5)
                : timeout;
        }
    }

    private sealed class StdioPluginContext : IISkyProPluginV2Context, IPluginV2MessageTransport
    {
        private readonly HostConnection _connection;

        public StdioPluginContext(HostConnection connection, string pluginId)
        {
            _connection = connection;
            PluginId = pluginId;
        }

        public string PluginId { get; }

        public IMessageService Messages => new MessageService(this);

        public async ValueTask ReplyAsync(
            PluginSdkV2MessageReference reference,
            OutgoingMessage message,
            CancellationToken cancellationToken)
        {
            var normalized = SdkOutgoingMessageNormalizer.Normalize(message);
            _ = await InvokeWithResultAsync(
                PluginSdkV2Protocol.MessagesReplyMethod,
                new Dictionary<string, object?>
                {
                    ["reference"] = reference,
                    ["message"] = normalized
                },
                cancellationToken);
        }

        public async ValueTask SendAsync(
            MessageTarget target,
            OutgoingMessage message,
            CancellationToken cancellationToken)
        {
            var normalized = SdkOutgoingMessageNormalizer.Normalize(message);
            _ = await InvokeWithResultAsync(
                PluginSdkV2Protocol.MessagesSendMethod,
                new Dictionary<string, object?>
                {
                    ["target"] = target,
                    ["message"] = normalized
                },
                cancellationToken);
        }

        public async ValueTask InvokeAsync(
            string method,
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            _ = await InvokeWithResultAsync(method, parameters, cancellationToken);
        }

        public ValueTask<JsonElement> InvokeWithResultAsync(
            string method,
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            return _connection.InvokeAsync(method, parameters, cancellationToken);
        }

        public async ValueTask WriteLogAsync(
            string level,
            string message,
            CancellationToken cancellationToken)
        {
            _ = await InvokeWithResultAsync(
                PluginSdkV2Protocol.LogWriteMethod,
                new Dictionary<string, object?>
                {
                    ["level"] = string.IsNullOrWhiteSpace(level) ? "Information" : level,
                    ["message"] = message
                },
                cancellationToken);
        }
    }
}

public sealed class PluginSdkV2RpcException : Exception
{
    public PluginSdkV2RpcException(int code, string message, JsonElement? data = null)
        : base(message)
    {
        Code = code;
        DataElement = data;
    }

    public int Code { get; }

    public JsonElement? DataElement { get; }
}
