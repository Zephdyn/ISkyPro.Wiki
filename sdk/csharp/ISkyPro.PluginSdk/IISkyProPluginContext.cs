using ISkyPro.Contracts.BotModels;

namespace ISkyPro.PluginSdk;

public interface IISkyProPluginContext
{
    string PluginId { get; }

    ValueTask WriteLogAsync(string level, string message, CancellationToken cancellationToken);

    ValueTask SendTextAsync(
        BotMessageKind targetKind,
        string targetId,
        string content,
        CancellationToken cancellationToken);
}
