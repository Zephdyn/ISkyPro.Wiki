using System.Text.Json.Serialization;

namespace ISkyPro.Contracts.PluginModels;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextPart), "text")]
[JsonDerivedType(typeof(MentionPart), "mention")]
[JsonDerivedType(typeof(CompositePart), "composite")]
[JsonDerivedType(typeof(ImagePart), "image")]
[JsonDerivedType(typeof(ImageUrlPart), "image-url")]
public abstract record MessagePart
{
    public static implicit operator MessagePart(string text) => new TextPart(text);
}

public sealed record TextPart(string Text) : MessagePart;

/// <summary>本地图片路径(compat 路径,Main 以 base64 file_data 上传)。</summary>
public sealed record ImagePart(string FilePath) : MessagePart;

/// <summary>远程图片 URL(官方推荐路径,Main 以 url 直传方式上传后按富媒体发送)。</summary>
public sealed record ImageUrlPart(string Url) : MessagePart;

public sealed record MentionPart(
    MentionTarget Target,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Id = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] QqBotMentionFormat? QqBotFormat = null) : MessagePart;

public sealed record CompositePart(IReadOnlyList<MessagePart> Parts) : MessagePart;

[JsonConverter(typeof(JsonStringEnumConverter<MentionTarget>))]
public enum MentionTarget
{
    [JsonStringEnumMemberName("user")]
    User,

    [JsonStringEnumMemberName("everyone")]
    Everyone
}

[JsonConverter(typeof(JsonStringEnumConverter<QqBotMentionFormat>))]
public enum QqBotMentionFormat
{
    [JsonStringEnumMemberName("current")]
    Current,

    [JsonStringEnumMemberName("legacy")]
    Legacy,

    [JsonStringEnumMemberName("legacy-bang")]
    LegacyBang
}

public sealed record UserRef(
    string Provider,
    string MentionId,
    string? UserOpenId,
    string? MemberOpenId,
    string? UnionOpenId,
    string? DisplayName);

[JsonConverter(typeof(JsonStringEnumConverter<OutgoingMessageFormat>))]
public enum OutgoingMessageFormat
{
    [JsonStringEnumMemberName("text")]
    Text,

    [JsonStringEnumMemberName("markdown")]
    Markdown
}

public sealed record OutgoingMessage
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public OutgoingMessageFormat Format { get; init; }

    public IReadOnlyList<MessagePart> Parts { get; init; } = Array.Empty<MessagePart>();
}

public sealed record MessageTarget(
    string Type,
    string Id,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    string? Platform = null,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    string? BotAccountId = null);

public sealed record MessageSendResult(
    bool Accepted,
    string? MessageId,
    MessageTarget Target);

public sealed record PluginSdkV2RecallRequest(
    MessageTarget Target,
    string MessageId);

public sealed record MessageRecallResult(
    bool Accepted,
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
    public const string FormatUnsupportedTarget = "message.format.unsupported_target";
    public const string ImageInvalid = "message.image.invalid";
    public const string ImageTooMany = "message.image.too_many";
    public const string ImageFormatUnsupported = "message.image.format_unsupported";
    public const string TargetInvalid = "message.target.invalid";
    public const string PlatformNotSupported = "message.target.platform_not_supported";
    public const string AccountNotFound = "message.target.account_not_found";
    public const string ReferenceExpired = "message.reference.expired";
}
