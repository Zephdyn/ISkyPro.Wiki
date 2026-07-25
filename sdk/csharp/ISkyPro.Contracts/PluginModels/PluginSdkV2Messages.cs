using System.Text.Json.Serialization;

namespace ISkyPro.Contracts.PluginModels;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextPart), "text")]
[JsonDerivedType(typeof(MentionPart), "mention")]
[JsonDerivedType(typeof(CompositePart), "composite")]
public abstract record MessagePart
{
    public static implicit operator MessagePart(string text) => new TextPart(text);
}

public sealed record TextPart(string Text) : MessagePart;

public sealed record MentionPart(
    MentionTarget Target,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Id = null) : MessagePart;

public sealed record CompositePart(IReadOnlyList<MessagePart> Parts) : MessagePart;

[JsonConverter(typeof(JsonStringEnumConverter<MentionTarget>))]
public enum MentionTarget
{
    [JsonStringEnumMemberName("user")]
    User,

    [JsonStringEnumMemberName("everyone")]
    Everyone
}

public sealed record UserRef(
    string Provider,
    string MentionId,
    string? UserOpenId,
    string? MemberOpenId,
    string? UnionOpenId,
    string? DisplayName);

public sealed record OutgoingMessage
{
    public IReadOnlyList<MessagePart> Parts { get; init; } = Array.Empty<MessagePart>();
}

public sealed record MessageTarget(string Type, string Id);

public sealed record MessageSendResult(
    bool Accepted,
    string? MessageId,
    MessageTarget Target);

public sealed record PluginSdkV2ReplyRequest(
    PluginSdkV2MessageReference Reference,
    OutgoingMessage Message);

public sealed record PluginSdkV2SendRequest(
    MessageTarget Target,
    OutgoingMessage Message);

public static class PluginSdkV2MessageTargetTypes
{
    public const string Group = "group";
    public const string Channel = "channel";
    public const string User = "user";
    public const string DirectMessage = "direct";
}

public static class PluginSdkV2MessageErrorCodes
{
    public const string Empty = "message.empty";
    public const string TooLong = "message.too_long";
    public const string PartsTooMany = "message.parts.too_many";
    public const string PartUnsupported = "message.part.unsupported";
    public const string MentionInvalidId = "message.mention.invalid_id";
    public const string MentionUnsupportedTarget = "message.mention.unsupported_target";
    public const string MentionEveryoneUnsupportedTarget = "message.mention_everyone.unsupported_target";
    public const string MentionEveryonePermissionDenied = "message.mention_everyone.permission_denied";
    public const string TargetInvalid = "message.target.invalid";
    public const string ReferenceExpired = "message.reference.expired";
}
