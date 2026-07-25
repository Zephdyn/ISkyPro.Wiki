using ISkyPro.Contracts.PluginModels;
using ISkyPro.PluginSdk.V2;

namespace ISkyPro.SamplePlugin;

public sealed class EchoPluginV2 : IISkyProPluginV2
{
    public async ValueTask<PluginSdkV2EventAck> OnMessageAsync(
        MessageContext message,
        IISkyProPluginV2Context context,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(message.EventType, "message.created", StringComparison.Ordinal))
        {
            return new PluginSdkV2EventAck(message.EventId, Accepted: true, Error: null);
        }

        var content = message.Text.Trim();
        if (content.Length == 0)
        {
            return new PluginSdkV2EventAck(message.EventId, Accepted: true, Error: null);
        }

        var rawMessageId = message.RawPayload.TryGetProperty("id", out var id)
            ? id.GetString()
            : message.Id;
        await context.WriteLogAsync("Information", $"Echo v2 received {rawMessageId}.", cancellationToken);
        await message.ReplyAsync(cancellationToken, $"echo: {content}");
        await context.InvokeAsync(
            "users.getCurrentBot",
            new Dictionary<string, object?>(),
            cancellationToken);

        return new PluginSdkV2EventAck(message.EventId, Accepted: true, Error: null);
    }
}
