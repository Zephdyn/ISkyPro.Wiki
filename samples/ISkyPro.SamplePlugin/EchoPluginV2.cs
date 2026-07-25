using ISkyPro.Contracts.PluginModels;
using ISkyPro.PluginSdk.V2;

namespace ISkyPro.SamplePlugin;

public sealed class EchoPluginV2 : IISkyProPluginV2
{
    public async ValueTask<PluginSdkV2EventAck> OnEventAsync(
        PluginSdkV2EventEnvelope pluginEvent,
        IISkyProPluginV2Context context,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(pluginEvent.EventType, "message.created", StringComparison.Ordinal))
        {
            return new PluginSdkV2EventAck(pluginEvent.EventId, Accepted: true, Error: null);
        }

        var content = pluginEvent.Message.Content.Trim();
        if (content.Length == 0)
        {
            return new PluginSdkV2EventAck(pluginEvent.EventId, Accepted: true, Error: null);
        }

        var rawMessageId = pluginEvent.RawPayload.TryGetProperty("id", out var id)
            ? id.GetString()
            : pluginEvent.Message.Id;
        await context.WriteLogAsync("Information", $"Echo v2 received {rawMessageId}.", cancellationToken);
        await context.ReplyTextAsync(pluginEvent.MessageReference, $"echo: {content}", cancellationToken);
        await context.InvokeAsync(
            "users.getCurrentBot",
            new Dictionary<string, object?>(),
            cancellationToken);

        return new PluginSdkV2EventAck(pluginEvent.EventId, Accepted: true, Error: null);
    }
}
