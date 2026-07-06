# 2.0.0-preview.2

发布日期：2026-07-06

## Improvements

- service: 服务菜单和 `service\*.bat` 脚本默认根据 Windows 语言和区域设置显示中文或英文，环境变量仅保留为高级用户临时覆盖入口。
- docs: 更新服务部署和启动说明，明确普通用户不需要修改环境变量。
- logs: 旧插件发送消息日志现在保留原始 `msg` 内容，便于排查 `[pic,file=...]`、QQBot 内嵌文本等兼容内容。
- webui: Bot 会话消息明细表调整列宽，序号、时间、方向、发送方和事件列保持可读，消息列使用剩余宽度。

## Compatibility

- legacy-plugin: 旧易语言图片字符串码 `[pic,file=...]` 现在会在 QQBot 发送层转换为本地图片发送；`<@id>`、`<@!id>`、`@everyone` 等 QQBot 内嵌格式保持原字符串透传给腾讯服务器处理。
- legacy-plugin: 群聊 `Api_SendMsg_v2(Type=4)` 的 `ADD/users_openid` 不再作为 QQBot 不支持的字段原样发送，也不会自动生成 At。
- legacy-plugin: `Api.取_消息_MD(...)` 构造的 Markdown 消息现在会转换为 QQBot `markdown`/`keyboard` 请求体；频道、频道私信、群聊和单聊发送会在 `msg_id` 为空时按主动消息发送，不再误报 `msg_id` 缺失。
