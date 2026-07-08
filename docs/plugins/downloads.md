# SDK 下载

SDK 压缩包随同一产品版本 GitHub Release 发布。`2.0.0-preview.3` 的 SDK 仍是 preview API，不承诺稳定命名冻结。

下载入口：

- [v2.0.0-preview.3 Release](https://github.com/Zephdyn/ISkyPro.Wiki/releases/tag/v2.0.0-preview.3)

主程序发布包命名：

| 平台 | 文件名 |
| --- | --- |
| Windows x64 | `ISkyPro-2.0.0-preview.3-win-x64.zip` |
| Windows ARM64 | `ISkyPro-2.0.0-preview.3-win-arm64.zip` |
| Linux x64 preview | `ISkyPro-2.0.0-preview.3-linux-x64.tar.gz` |

建议 asset 命名：

| SDK | 文件名 |
| --- | --- |
| 旧插件 SDK | `ISkyPro-LegacySdk-2.0.0-preview.3.zip` |
| C# Plugin SDK v2 | `ISkyPro-PluginSdk-CSharp-2.0.0-preview.3.zip` |
| Python Plugin SDK v2 | `ISkyPro-PluginSdk-Python-2.0.0-preview.3.zip` |
| Node.js Plugin SDK v2 | `ISkyPro-PluginSdk-Node-2.0.0-preview.3.zip` |
| Go Plugin SDK v2 | `ISkyPro-PluginSdk-Go-2.0.0-preview.3.zip` |

这些 SDK 包不是主程序更新包。主程序发布包会带 `win-x64`、`win-arm64` 或 `linux-x64` 等运行时标识，SDK 包不应使用这些主程序运行时标识。

开发时也可以直接参考仓库样例：

- `samples/ISkyPro.SamplePlugin`
- `samples/stdio-python-plugin`
- `samples/stdio-node-plugin`
- `samples/stdio-go-plugin`
