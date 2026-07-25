using ISkyPro.Contracts.PluginModels;

namespace ISkyPro.PluginSdk.V2;

public interface IISkyProPluginV2
{
    ValueTask<PluginSdkV2EventAck> OnMessageAsync(
        MessageContext message,
        IISkyProPluginV2Context context,
        CancellationToken cancellationToken);
}
