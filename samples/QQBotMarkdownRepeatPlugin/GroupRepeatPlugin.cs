using ISkyPro.Contracts.PluginModels;
using ISkyPro.PluginSdk.V2;

namespace RepeatPlugin;

public sealed class GroupRepeatPlugin : IISkyProPluginV2
{
    private const string RepeatPrefix = "复读";

    public async ValueTask<PluginSdkV2EventAck> OnMessageAsync(
        MessageContext message,
        IISkyProPluginV2Context context,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                message.Conversation.Type,
                PluginSdkV2MessageTargetTypes.Group,
                StringComparison.Ordinal) ||
            !message.Text.StartsWith(RepeatPrefix, StringComparison.Ordinal))
        {
            return Accepted(message.EventId);
        }

        var repeatedText = message.Text[RepeatPrefix.Length..].TrimStart();
        if (repeatedText.Length == 0)
        {
            return Accepted(message.EventId);
        }

        if (string.IsNullOrWhiteSpace(message.Sender.MentionId))
        {
            return new PluginSdkV2EventAck(
                message.EventId,
                Accepted: false,
                Error: "The group message did not contain a sender mention id.");
        }

        await message.ReplyMarkdownAsync(
            cancellationToken,
            At.User(message.Sender),
            " ",
            repeatedText);

        return Accepted(message.EventId);
    }

    private static PluginSdkV2EventAck Accepted(string eventId)
        => new(eventId, Accepted: true, Error: null);
}
