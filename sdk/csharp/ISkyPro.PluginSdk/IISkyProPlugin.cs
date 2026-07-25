using ISkyPro.Contracts.PluginModels;

namespace ISkyPro.PluginSdk;

public interface IISkyProPlugin
{
    ModernPluginManifest Manifest { get; }

    ValueTask<ModernPluginMessageResponse> OnMessageAsync(
        ModernPluginMessageEvent pluginEvent,
        IISkyProPluginContext context,
        CancellationToken cancellationToken);
}
