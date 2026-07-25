namespace ISkyPro.Contracts.PluginModels;

[Flags]
public enum ModernPluginCapability
{
    None = 0,
    ReceiveMessages = 1,
    SendMessages = 2,
    InterceptMessages = 4,
    HttpTransport = 8
}
