# 插件故障排查

## 新插件未发现

- zip 根目录或唯一顶层目录下没有 `manifest.json`。
- manifest JSON 格式错误。
- `pluginId`、`name`、`version`、`author` 或 `sdkVersion` 缺失。
- `transport.type` 不是 `stdio-jsonrpc`。
- 插件安装到了旧插件 `plugin/` 目录，而不是通过新插件上传入口安装。

## 启动失败

- `transport.stdio.command` 不存在或不在 PATH。
- `workingDirectory` 指向错误。
- Python / Node.js / Go 没有安装。
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

SDK API 调用会按 manifest `permissions` 校验。调用 `messages.replyText` 需要 `messages.reply`，读取当前机器人资料需要 `users.read`。

## 回复失败

- 事件缺少可用 `messageReference`。
- 回复超过平台允许窗口。
- Bot 没有目标会话权限。
- 插件声明了权限，但 Bot 平台侧没有对应能力。
- 发送队列或平台 API 返回错误，需查看 WebUI 日志。

## 群消息收不到

先看 [QQBot 事件配置](/guide/qqbot-events)。重点检查群全量消息开关、`GROUP_MESSAGE_CREATE` 事件勾选、连接模式和插件日志。
