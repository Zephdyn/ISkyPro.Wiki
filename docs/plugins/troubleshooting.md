# 插件故障排查

## ISkyPro v2+ 插件未发现

- zip 根目录或唯一顶层目录下没有 `manifest.json`。
- manifest JSON 格式错误。
- `pluginId`、`name`、`version`、`author` 或 `sdkVersion` 缺失。
- `transport.type` 不是 `stdio-jsonrpc`。
- 插件被复制到 ISky v1 插件使用的 `plugin/` 目录，而不是通过 v2 插件上传入口安装。

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

这些都会污染 stdout。请改为 stderr。

## ACK 超时

插件收到事件后应尽快返回 ACK。如果需要慢 HTTP、数据库或长任务，先 ACK，再在插件内部异步处理并调用 SDK 方法回复。

## 无权限

SDK API 调用会按 manifest `permissions` 校验。调用 `messages.reply` 需要 `messages.reply`，主动发送需要 `messages.send`，撤回需要 `messages.recall`，读取当前机器人资料需要 `users.read`。QQ 平台错误（如 22009 消息超频）会以 JSON-RPC error `data.errorCode`（`qq.api.<errCode>`）返回，插件可以按错误码分支处理。

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
