using ISkyPro.Contracts.PluginModels;
using System.Text.Json;

namespace ISkyPro.PluginSdk.V2;

public interface IISkyProPluginV2Context
{
    string PluginId { get; }

    ValueTask ReplyTextAsync(
        PluginSdkV2MessageReference messageReference,
        string content,
        CancellationToken cancellationToken);

    ValueTask InvokeAsync(
        string method,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken);

    ValueTask<JsonElement> InvokeWithResultAsync(
        string method,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken);

    ValueTask WriteLogAsync(
        string level,
        string message,
        CancellationToken cancellationToken);
}
