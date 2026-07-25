namespace ISkyPro.Contracts.PluginModels;

public static class PluginSdkV2Protocol
{
    public const int Version = ModernPluginProtocol.Version2;
    public const string JsonRpcVersion = "2.0";
    public const string EncodingJson = "json";
    public const string TransportStdioJsonRpc = "stdio-jsonrpc";
    public const string TransportHttp = "http";
    public const string InitializeMethod = "iskypro.initialize";
    public const string StopMethod = "plugin.stop";
    public const string ShutdownMethod = "shutdown";
    public const string LogWriteMethod = "log.write";
    public const string MessageEventMethod = "events.message";
    public const string MessagesReplyMethod = "messages.reply";
    public const string MessagesSendMethod = "messages.send";

    public const int DefaultRequestTimeoutMilliseconds = 30_000;
    public const int MaxHeaderLength = 8 * 1024;
    public const int MaxPayloadLength = 4 * 1024 * 1024;

    public const string CapabilityBidirectionalRequests = "bidirectional-requests";
    public const string CapabilityConcurrentEvents = "concurrent-events";
    public const string CapabilityGracefulShutdown = "graceful-shutdown";

    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    public const int PermissionDenied = -32001;
    public const int PluginError = -32000;
}

public static class PluginSdkV2Permissions
{
    public const string MessagesReply = "messages.reply";
    public const string MessagesSend = "messages.send";
    public const string MessagesMentionEveryone = "messages.mentionEveryone";
    public const string MediaUpload = "media.upload";
    public const string UsersRead = "users.read";
    public const string GroupsRead = "groups.read";
    public const string GroupsManage = "groups.manage";
    public const string GuildsRead = "guilds.read";
    public const string ChannelsManage = "channels.manage";
    public const string MembersRead = "members.read";
    public const string PermissionsManage = "permissions.manage";
    public const string UnsafeRawOpenApi = "unsafe.raw-open-api";
}

public static class PluginSdkV2OverflowPolicies
{
    public const string RejectNew = "reject-new";
    public const string DropOldest = "drop-oldest";
    public const string WarnOnly = "warn-only";
}
