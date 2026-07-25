# SDK 下载

Plugin SDK v2 的唯一源码位于本公开仓库的 `sdk/`，可直接 clone 或按语言下载同版本
Release 压缩包。稳定版 `2.0.0` 的 SDK 接口按语义化版本管理。

下载入口：

- [v2.0.0 Release](https://github.com/Zephdyn/ISkyPro.Wiki/releases/tag/v2.0.0)

主程序发布包命名：

| 平台 | 文件名 |
| --- | --- |
| Windows x64 | `ISkyPro-2.0.0-win-x64.zip` |
| Windows ARM64 | `ISkyPro-2.0.0-win-arm64.zip` |
| Linux x64 | `ISkyPro-2.0.0-linux-x64.tar.gz` |

SDK 压缩包文件名：

| SDK | 文件名 |
| --- | --- |
| 旧插件 SDK | `SDK-Legacy-2.0.0.zip` |
| C# Plugin SDK v2 | `SDK-V2-CSharp-2.0.0.zip` |
| Python Plugin SDK v2 | `SDK-V2-Python-2.0.0.zip` |
| Node.js Plugin SDK v2 | `SDK-V2-NodeJS-2.0.0.zip` |
| Go Plugin SDK v2 | `SDK-V2-Go-2.0.0.zip` |

SDK 压缩包用于插件开发，不用于安装或更新主程序。C# 包包含 NuGet 包、公共 Contracts、
SDK 源码和可运行样例；其他语言包保留 `sdk/<language>` 与 `samples/<sample>` 的相对布局，
解压后即可构建样例。
