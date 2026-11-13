# 插件故障排查

## ISkyPro v2+ 插件未发现

- zip 根目录或唯一顶层目录下没有 `manifest.json`。
- manifest JSON 格式错误。
- `pluginId`、`name`、`version`、`author` 或 `sdkVersion` 缺失。
- `transport.type` 不是 `stdio-jsonrpc`。
- 插件被复制到 ISky v1 插件使用的 `plugin/` 目录，而不是通过 v2 插件上传入口安装。

## ISky v1 插件载入失败 / 一直停在“启动中”

旧版本日志里可能出现：

```text
自动载入插件 cc.example.Plugin 失败：Plugin cc.example.Plugin is not initialized.
```

这条文案表示兼容宿主在载入时限内没有完成初始化，而不是插件不存在。2.1.0 起会直接给出阶段：

```text
插件载入超时：30000ms 内未完成初始化（未连接，未读取插件信息，未完成初始化）；宿主 PID 1234；宿主输出日志 data/logs/pluginhost/cc.example.Plugin-...log。已终止兼容运行器并按崩溃策略处理。
```

排查顺序：

1. 看日志或插件卡片里的失败原因，它指明了已到达的阶段。
2. 打开 `data/logs/pluginhost/` 下对应 `isky.exe` 的日志，看最后一条 `PluginHost status:` 行。
3. 未连接：多半是杀软拦截 `isky.exe` 或命名管道、磁盘过慢；已连接但没有“信息已读取”：`ISkyS_Information` 卡住；已读取信息但没有“初始化完成”：插件初始化自身阻塞（联网授权、弹窗、读写大文件）。
4. 确认是机器慢而不是插件坏时，可在 WebUI 设置页“运行时”把“载入超时 ms”调大（默认 30000），或设置 `Plugins:StartupTimeoutMilliseconds` 后重启。
5. 超时后宿主会被终止并按崩溃策略延迟重启；连续失败会进入暂停或停用，需要人工处理。

## 启动失败

- `transport.stdio.command` 不存在或不在 PATH。
- `workingDirectory` 指向错误。
- Python / Node.js 插件的目标机器没有对应运行时；Go 正式包应包含已编译程序，不应依赖 `go run`。
- ZIP 只包含源码，没有包含 Python SDK、Node.js `node_modules`、C# publish 依赖或 Go 可执行文件。
- 插件没有带 `--iskypro-stdio` 进入协议模式。
- initialize 返回的 `pluginId`、协议版本或 encoding 与 manifest 不一致。

## stdout 协议污染

stdio 插件的 stdout 只能写 JSON-RPC `Content-Length` 帧。普通日志必须写 stderr 或调用 `log.write`。

常见错误：

```text
print("hello")
console.log("hello")
fmt.Println("hello")
```

以上输出都会污染 stdout。插件日志应写入 stderr，或通过 SDK 的 `log.write` 输出。

## ACK 超时

插件收到事件后应尽快返回 ACK。如果插件需要耗时的 HTTP 调用、数据库操作或长任务，
应先 ACK，再在插件内部异步处理并调用 SDK 方法回复。

## 无权限

SDK API 调用会按 manifest 的 `permissions` 校验：回复事件需声明 `messages.reply`，
主动发送需声明 `messages.send`，撤回需声明 `messages.recall`，读取当前机器人资料
需声明 `users.read`。QQ 平台错误（如 22009 消息超频）以 JSON-RPC error 的
`data.errorCode`（`qq.api.<errCode>`）返回，可按错误码分支处理；错误码随 SDK 变更
更新，见 [SDK 更新日志](/changelog/sdk/)。

## HTTP 插件注册失败

- 把 HTTP 服务错误地打成 ZIP 上传；HTTP 插件应注册 Base URL。
- Base URL 无法从 ISkyPro 所在机器访问。
- `GET /iskypro/plugin/manifest` 返回空响应、无效 JSON 或缺少 `HttpTransport` capability。
- 消息接口没有实现 `POST /iskypro/plugin/events/message`。
- 反向代理、TLS 或额外鉴权阻止了 ISkyPro 请求。

## 回复失败

- 事件缺少可用 `messageReference`。
- 回复超过平台允许窗口。
- Bot 没有目标会话权限。
- 插件声明了权限，但 Bot 平台侧没有对应能力。
- 发送队列或平台 API 返回错误，需查看 WebUI 日志。

## 群消息收不到

先看 [QQBot 事件配置](/guide/qqbot-events)。重点检查群全量消息开关、`GROUP_MESSAGE_CREATE` 事件勾选、连接模式和插件日志。
