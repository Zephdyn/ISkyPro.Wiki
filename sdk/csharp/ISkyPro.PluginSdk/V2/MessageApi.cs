using System.Text.Json;
using ISkyPro.Contracts.BotModels;
using ISkyPro.Contracts.PluginModels;

namespace ISkyPro.PluginSdk.V2;

public static class At
{
    private const int MaxUsers = 20;

    public static MessagePart Everyone { get; } = new MentionPart(MentionTarget.Everyone);

    public static MessagePart User(string id, QqBotMentionFormat? qqBotFormat = null)
    {
        ValidateMentionId(id, nameof(id));
        ValidateQqBotFormatId(id, qqBotFormat, nameof(id));
        return new MentionPart(MentionTarget.User, id, qqBotFormat);
    }

    public static MessagePart User(UserRef user, QqBotMentionFormat? qqBotFormat = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        return User(user.MentionId, qqBotFormat);
    }

    public static MessagePart Users(
        IEnumerable<string> ids,
        string separator = " ",
        QqBotMentionFormat? qqBotFormat = null)
    {
        ArgumentNullException.ThrowIfNull(ids);
        return CreateUsers(ids.Select(id => User(id, qqBotFormat)), separator);
    }

    public static MessagePart Users(
        IEnumerable<UserRef> users,
        string separator = " ",
        QqBotMentionFormat? qqBotFormat = null)
    {
        ArgumentNullException.ThrowIfNull(users);
        return CreateUsers(users.Select(user => User(user, qqBotFormat)), separator);
    }

    private static MessagePart CreateUsers(IEnumerable<MessagePart> mentions, string separator)
    {
        ArgumentNullException.ThrowIfNull(separator);
        var items = mentions.ToArray();
        if (items.Length == 0)
        {
            throw new ArgumentException("At.Users requires at least one user.", nameof(mentions));
        }

        if (items.Length > MaxUsers)
        {
            throw new ArgumentException($"At.Users supports at most {MaxUsers} users.", nameof(mentions));
        }

        var parts = new List<MessagePart>((items.Length * 2) - 1);
        for (var index = 0; index < items.Length; index++)
        {
            if (index > 0 && separator.Length > 0)
            {
                parts.Add(new TextPart(separator));
            }

            parts.Add(items[index]);
        }

        return new CompositePart(parts);
    }

    private static void ValidateMentionId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsControl))
        {
            throw new ArgumentException("Mention id must not be empty or contain control characters.", parameterName);
        }
    }

    private static void ValidateQqBotFormatId(
        string id,
        QqBotMentionFormat? qqBotFormat,
        string parameterName)
    {
        if (qqBotFormat is QqBotMentionFormat.Legacy or QqBotMentionFormat.LegacyBang
            && id.IndexOfAny(['<', '>']) >= 0)
        {
            throw new ArgumentException(
                "Legacy QQBot mention ids must not contain '<' or '>'.",
                parameterName);
        }
    }
}

public interface IMessageService
{
    IMessageTarget Group(string groupOpenId);

    IMessageTarget Channel(string channelId);

    IMessageTarget User(string userOpenId);

    IMessageTarget DirectMessage(string guildId);
}

public interface IMessageTarget
{
    ValueTask SendAsync(params MessagePart[] parts);

    ValueTask SendAsync(CancellationToken cancellationToken, params MessagePart[] parts);

    ValueTask SendMarkdownAsync(params MessagePart[] parts);

    ValueTask SendMarkdownAsync(CancellationToken cancellationToken, params MessagePart[] parts);

    ValueTask SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default);
}

public sealed class MessageContext
{
    private readonly PluginSdkV2MessageReference _reference;
    private readonly IPluginV2MessageTransport _transport;
    private readonly CancellationToken _eventCancellationToken;

    internal MessageContext(
        PluginSdkV2EventEnvelope pluginEvent,
        IPluginV2MessageTransport transport,
        CancellationToken eventCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pluginEvent);
        ArgumentNullException.ThrowIfNull(transport);
        EventId = pluginEvent.EventId;
        EventType = pluginEvent.EventType;
        Timestamp = pluginEvent.Timestamp;
        Source = pluginEvent.Source;
        Bot = pluginEvent.Bot;
        Conversation = pluginEvent.Conversation;
        Sender = pluginEvent.Sender;
        Id = pluginEvent.Message.Id;
        Text = pluginEvent.Message.Content;
        Attachments = pluginEvent.Message.Attachments;
        Mentions = pluginEvent.Message.Mentions;
        RawPayload = pluginEvent.RawPayload;
        _reference = pluginEvent.MessageReference;
        _transport = transport;
        _eventCancellationToken = eventCancellationToken;
    }

    public string EventId { get; }

    public string EventType { get; }

    public DateTimeOffset Timestamp { get; }

    public string Source { get; }

    public string Id { get; }

    public BotAccountContext Bot { get; }

    public PluginSdkV2ConversationContext Conversation { get; }

    public UserRef Sender { get; }

    public string Text { get; }

    public IReadOnlyList<PluginSdkV2MessageAttachment> Attachments { get; }

    public IReadOnlyList<UserRef> Mentions { get; }

    public JsonElement RawPayload { get; }

    public ValueTask ReplyAsync(params MessagePart[] parts)
        => ReplyAsync(new OutgoingMessage { Parts = parts }, _eventCancellationToken);

    public ValueTask ReplyAsync(CancellationToken cancellationToken, params MessagePart[] parts)
        => ReplyAsync(new OutgoingMessage { Parts = parts }, cancellationToken);

    public ValueTask ReplyMarkdownAsync(params MessagePart[] parts)
        => ReplyAsync(
            new OutgoingMessage
            {
                Format = OutgoingMessageFormat.Markdown,
                Parts = parts
            },
            _eventCancellationToken);

    public ValueTask ReplyMarkdownAsync(CancellationToken cancellationToken, params MessagePart[] parts)
        => ReplyAsync(
            new OutgoingMessage
            {
                Format = OutgoingMessageFormat.Markdown,
                Parts = parts
            },
            cancellationToken);

    public ValueTask ReplyAsync(
        OutgoingMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!cancellationToken.CanBeCanceled || cancellationToken == _eventCancellationToken)
        {
            return _transport.ReplyAsync(_reference, message, _eventCancellationToken);
        }

        if (!_eventCancellationToken.CanBeCanceled)
        {
            return _transport.ReplyAsync(_reference, message, cancellationToken);
        }

        return ReplyWithLinkedCancellationAsync(message, cancellationToken);
    }

    private async ValueTask ReplyWithLinkedCancellationAsync(
        OutgoingMessage message,
        CancellationToken cancellationToken)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _eventCancellationToken,
            cancellationToken);
        await _transport.ReplyAsync(_reference, message, linkedCancellation.Token);
    }
}

internal interface IPluginV2MessageTransport
{
    ValueTask ReplyAsync(
        PluginSdkV2MessageReference reference,
        OutgoingMessage message,
        CancellationToken cancellationToken);

    ValueTask SendAsync(
        MessageTarget target,
        OutgoingMessage message,
        CancellationToken cancellationToken);
}

internal sealed class MessageService : IMessageService
{
    private readonly IPluginV2MessageTransport _transport;

    public MessageService(IPluginV2MessageTransport transport)
    {
        _transport = transport;
    }

    public IMessageTarget Group(string groupOpenId)
        => Create(PluginSdkV2MessageTargetTypes.Group, groupOpenId);

    public IMessageTarget Channel(string channelId)
        => Create(PluginSdkV2MessageTargetTypes.Channel, channelId);

    public IMessageTarget User(string userOpenId)
        => Create(PluginSdkV2MessageTargetTypes.User, userOpenId);

    public IMessageTarget DirectMessage(string guildId)
        => Create(PluginSdkV2MessageTargetTypes.DirectMessage, guildId);

    private IMessageTarget Create(string type, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsControl))
        {
            throw new ArgumentException("Message target id must not be empty or contain control characters.", nameof(id));
        }

        return new MessageTargetClient(_transport, new MessageTarget(type, id));
    }
}

internal sealed class MessageTargetClient : IMessageTarget
{
    private readonly IPluginV2MessageTransport _transport;
    private readonly MessageTarget _target;

    public MessageTargetClient(IPluginV2MessageTransport transport, MessageTarget target)
    {
        _transport = transport;
        _target = target;
    }

    public ValueTask SendAsync(params MessagePart[] parts)
        => SendAsync(new OutgoingMessage { Parts = parts });

    public ValueTask SendAsync(CancellationToken cancellationToken, params MessagePart[] parts)
        => SendAsync(new OutgoingMessage { Parts = parts }, cancellationToken);

    public ValueTask SendMarkdownAsync(params MessagePart[] parts)
        => SendAsync(new OutgoingMessage
        {
            Format = OutgoingMessageFormat.Markdown,
            Parts = parts
        });

    public ValueTask SendMarkdownAsync(CancellationToken cancellationToken, params MessagePart[] parts)
        => SendAsync(
            new OutgoingMessage
            {
                Format = OutgoingMessageFormat.Markdown,
                Parts = parts
            },
            cancellationToken);

    public ValueTask SendAsync(
        OutgoingMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _transport.SendAsync(_target, message, cancellationToken);
    }
}

internal static class SdkOutgoingMessageNormalizer
{
    public static OutgoingMessage Normalize(OutgoingMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var flattened = new List<MessagePart>();
        foreach (var part in message.Parts ?? Array.Empty<MessagePart>())
        {
            Flatten(part, flattened);
        }

        var normalized = new List<MessagePart>(flattened.Count);
        foreach (var part in flattened)
        {
            if (part is TextPart { Text.Length: 0 })
            {
                continue;
            }

            if (part is TextPart text
                && normalized.LastOrDefault() is TextPart previous)
            {
                normalized[^1] = new TextPart(previous.Text + text.Text);
                continue;
            }

            normalized.Add(part);
        }

        if (normalized.Count == 0)
        {
            throw new ArgumentException("Message must contain at least one non-empty part.", nameof(message));
        }

        return message with { Parts = normalized };
    }

    private static void Flatten(MessagePart? part, List<MessagePart> target)
    {
        switch (part)
        {
            case null:
                throw new ArgumentException("Message parts must not contain null values.", nameof(part));
            case CompositePart composite:
                foreach (var child in composite.Parts ?? Array.Empty<MessagePart>())
                {
                    Flatten(child, target);
                }
                break;
            case TextPart or MentionPart:
                target.Add(part);
                break;
            default:
                throw new ArgumentException($"Unsupported message part: {part.GetType().Name}.", nameof(part));
        }
    }
}
