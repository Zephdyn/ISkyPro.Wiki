using System.Text.Json;
using ISkyPro.Contracts.BotModels;
using ISkyPro.Contracts.PluginModels;
using ISkyPro.PluginSdk;

namespace ISkyPro.SamplePlugin;

public sealed class EchoPlugin : IISkyProPlugin
{
    public ModernPluginManifest Manifest { get; } = new(
        PluginId: "top.iskypro.sample.echo",
        Name: "ISkyPro Echo Sample",
        Version: "0.1.0",
        Author: "ISkyPro",
        ProtocolVersion: ModernPluginProtocol.Version,
        Capabilities: ModernPluginCapability.ReceiveMessages
            | ModernPluginCapability.SendMessages
            | ModernPluginCapability.HttpTransport);

    public async ValueTask<ModernPluginMessageResponse> OnMessageAsync(
        ModernPluginMessageEvent pluginEvent,
        IISkyProPluginContext context,
        CancellationToken cancellationToken)
    {
        var content = TryGetContent(pluginEvent.Message.Payload);
        if (string.IsNullOrWhiteSpace(content))
        {
            return ModernPluginMessageResponse.Handled();
        }

        await context.WriteLogAsync("Information", $"Echoing message {pluginEvent.Message.Id}.", cancellationToken);
        await context.SendTextAsync(
            pluginEvent.Message.Kind,
            pluginEvent.Message.Id,
            $"echo: {content}",
            cancellationToken);

        return ModernPluginMessageResponse.Handled(
            outboundMessages: context is InMemoryPluginContext memoryContext
                ? memoryContext.OutboundMessages
                : null);
    }

    private static string? TryGetContent(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Object
            && payload.TryGetProperty("d", out var data)
            && data.ValueKind == JsonValueKind.Object
            && data.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.String)
        {
            return content.GetString();
        }

        return null;
    }
}
