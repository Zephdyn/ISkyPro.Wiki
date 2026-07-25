using ISkyPro.PluginSdk.V2;
using ISkyPro.SamplePlugin;

if (!args.Contains("--iskypro-stdio", StringComparer.Ordinal))
{
    Console.Error.WriteLine(
        "This plugin is meant to be run by ISkyPro with --iskypro-stdio.");
    return 2;
}

await StdioPluginV2Host.RunAsync(new EchoPluginV2());
return 0;
