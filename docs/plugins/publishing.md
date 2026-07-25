# 发布插件

发布方式由 transport 决定：

| Transport | 发布产物 | 用户安装方式 |
| --- | --- | --- |
| `stdio-jsonrpc` | 可直接运行的插件 ZIP | WebUI 上传，ISkyPro 管理启动、停止和重启 |
| HTTP | 已部署并持续运行的 HTTP 服务 | WebUI 注册 Base URL，不上传 ZIP |

## 发布 stdio-jsonrpc 插件

本地安装包使用 ZIP。包内必须包含静态 `manifest.json`、插件入口和所有运行依赖。

支持两种结构：

```text
manifest.json
plugin.py
README.md
```

或：

```text
top.example.echo/
  manifest.json
  plugin.py
  README.md
```

安装后会规范化为：

```text
plugins-v2/
  top.example.echo/
    manifest.json
    plugin.py
```

## manifest 要点

- `pluginId` 必须稳定，更新版本时不要随意更改。
- `protocolVersion` 使用 `2`。
- 本地包安装第一阶段只接受 `stdio-jsonrpc`。
- `transport.stdio.args` 建议包含 `--iskypro-stdio`。
- `permissions` 只声明实际需要的权限。
- 需要设置表单时，在 `settings.configSchema` 中声明字段。

## 各语言推荐命令

### C#

官方 NuGet 包带有 publish target。项目根目录存在 `manifest.json` 时：

```powershell
dotnet publish .\MyPlugin.csproj -c Release
```

默认生成 `artifacts/<AssemblyName>.zip`。可以通过以下属性调整：

```xml
<PropertyGroup>
  <ISkyProPackagePluginOnPublish>true</ISkyProPackagePluginOnPublish>
  <ISkyProPluginManifest>manifest.json</ISkyProPluginManifest>
  <ISkyProPluginPackagePath>artifacts/MyPlugin.zip</ISkyProPluginPackagePath>
</PropertyGroup>
```

### Python

参考 `samples/stdio-python-plugin/package.py`。打包时需要把 `iskypro_sdk_v2` 和其他第三方依赖一同放入 ZIP，不能依赖源码仓库中的相对路径。

### Node.js

参考 `samples/stdio-node-plugin/package-plugin.mjs`，运行 `npm run package:plugin`。正式包应包含运行所需的 `node_modules`，不能假定用户机器存在全局 SDK 包。

### Go

参考 `samples/stdio-go-plugin/tools/package-plugin`，运行 `go run ./tools/package-plugin`。为每个目标 OS/架构分别编译，包内 manifest 必须启动已编译程序，而不是使用 `go run`。

## 上传安装

在 WebUI 新插件页上传 zip。安装器会：

- 拒绝路径穿越条目。
- 只读取 manifest，不执行插件。
- 校验 manifest。
- 拒绝覆盖运行中的插件。
- 更新时保留旧版本备份。

默认不删除插件的 data/config。删除数据应由用户明确执行，不应作为普通更新的一部分。

## 发布 HTTP 插件

HTTP 插件不生成 ISkyPro 安装 ZIP。开发者应先把服务部署到服务器、容器平台或本机服务管理器，并确保 ISkyPro 可以访问其 Base URL。

必须提供：

- `GET /iskypro/plugin/manifest`：返回 `ModernPluginManifest`，并声明 `ReceiveMessages` 与 `HttpTransport` capability。
- `POST /iskypro/plugin/events/message`：接收消息事件并返回 `ModernPluginMessageResponse`。

用户在 WebUI“新插件”页输入 Base URL 完成注册。发布新版本时由服务自身完成滚动更新或重启，不通过 ISkyPro ZIP 覆盖流程。

HTTP 发布说明还应写清：

- 服务地址和网络可达性要求。
- 是否需要 HTTPS、反向代理或额外鉴权。
- 超时、并发和重试策略。
- 服务日志与健康检查位置。
- 更新、回滚和高可用方式。

## 发布说明

发布页或 README 中建议写清：

- 支持的 ISkyPro 版本。
- 插件权限用途。
- 需要的运行时，例如 Python、Node.js 或 Go。
- 配置项说明。
- 常见错误和日志位置。
- transport 类型，以及用户应该上传 ZIP 还是注册 Base URL。
