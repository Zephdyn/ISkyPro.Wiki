# QQBot Markdown @ Repeat Sample

这是一个可公开复用的 C# Plugin SDK v2 示例。群聊用户发送：

```text
复读 你好
```

插件通过 `ReplyMarkdownAsync` 回复发送者，Main 将类型化 `At.User(...)` 渲染为
`<qqbot-at-user id="member_openid" />`，并按 QQ 群消息协议发送
`msg_type = 2` 与 `markdown.content`。插件不拼接平台标签，用户输入中的标签字符仍会
被转义。

> 兼容性记录（2026-07-25）：在当日的 QQ 群实机测试中，普通文本
> `msg_type = 0` 发送 `<qqbot-at-user ... />`、`<@id>` 或 `<@!id>` 都会显示为
> 普通文本；改用 Markdown 的 `msg_type = 2` 和 `markdown.content` 后，
> `<qqbot-at-user ... />` 可以正常触发 @。这是当时服务端与客户端行为的实测记录，
> 不是永久兼容保证；腾讯后续可能调整解析规则，请同时核对最新官方文档和客户端表现。

官方接口参考：[发送群消息](https://bot.q.qq.com/wiki/develop/api-v2/autogen/api/v2_groups_group_openid_messages.post.html#schema-messagemarkdown)。

发布并生成 WebUI 可安装 ZIP：

```powershell
dotnet publish .\QQBotMarkdownRepeatPlugin.csproj -c Release
```

输出为 `artifacts/RepeatPlugin.zip`。

---

This public C# Plugin SDK v2 sample replies to `复读 hello` with a typed mention
inside a QQ group Markdown message. The compatibility note above records behavior
observed on 2026-07-25; re-check current QQBot documentation and clients because
the platform may change its parsing rules.
