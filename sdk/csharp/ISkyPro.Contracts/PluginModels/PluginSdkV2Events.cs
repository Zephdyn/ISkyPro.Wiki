using System.Text.Json;
using ISkyPro.Contracts.BotModels;

namespace ISkyPro.Contracts.PluginModels;

public sealed record PluginSdkV2EventEnvelope(
    string EventId,
    string EventType,
    DateTimeOffset Timestamp,
    string Source,
    BotAccountContext Bot,
    PluginSdkV2ConversationContext Conversation,
    PluginSdkV2SenderContext Sender,
    PluginSdkV2MessageContext Message,
    PluginSdkV2MessageReference MessageReference,
    JsonElement RawPayload);

public sealed record PluginSdkV2ConversationContext(
    string Type,
    string ConversationId,
    string? GuildId,
    string? ChannelId,
    string? GroupOpenId,
    string? C2COpenId);

public sealed record PluginSdkV2SenderContext(
    string? UserOpenId,
    string? MemberOpenId,
    string? UnionOpenId,
    string? DisplayName);

public sealed record PluginSdkV2MessageContext(
    string Id,
    string Content,
    IReadOnlyList<PluginSdkV2MessageAttachment> Attachments,
    IReadOnlyList<string> Mentions);

public sealed record PluginSdkV2MessageAttachment(
    string Id,
    string ContentType,
    string? Url,
    long? SizeBytes);

public sealed record PluginSdkV2MessageReference(
    string MessageId,
    string TargetType,
    string TargetId,
    DateTimeOffset? ReplyUntil,
    IReadOnlyList<string> Restrictions);

public sealed record PluginSdkV2EventAck(
    string EventId,
    bool Accepted,
    string? Error);

public sealed record PluginSdkV2Error(
    string Code,
    string Message,
    JsonElement? Details);
