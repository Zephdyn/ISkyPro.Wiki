# 2.1.0-preview.2 SDK

状态：开发中，暂未发布

本版本为多平台多账号做准备：消息目标可显式指定账号，新增平台时 SDK 与协议零改动。

## ✨ 新功能

- messages: 消息目标支持可选 `botAccountId`（格式 `"{平台}:{账号ID}"`）：`messages.reply` 的事件引用与 `messages.send` / `messages.recall` 的 `target` 可显式指定目标账号；不传则使用平台默认账号，旧插件无需改动、行为不变。
- messages: 事件引用自动携带事件来源账号，未指定目标账号时回复默认回到来源账号。
- errors: 平台未注册或账号不存在时返回稳定错误码 `message.target.platform_not_supported` / `message.target.account_not_found`。
- api: 四语言 SDK（C# / Node.js / Python / Go）消息 API 增加可选账号参数，为同时登录多平台多账号预留。

## ♻️ 兼容性

- 协议新增字段全部可选，向后兼容；已编译旧插件无需重新发布。
- 新增平台（微信、飞书、钉钉、Telegram、Discord 等）只需在 Main 注册网关与适配器，SDK 与协议无需改动。