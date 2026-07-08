# 2.0.0-preview.2

Release date: 2026-07-06

## Improvements

- service: The service menu and `service\*.bat` scripts now default to the Windows language and regional settings for Chinese or English output, with environment variables kept only as an advanced temporary override.
- docs: Updated service deployment and startup documentation to clarify that normal users do not need to edit environment variables.
- logs: Legacy plugin send logs now preserve the original `msg` content, making `[pic,file=...]`, QQBot embedded text, and other compatibility text easier to debug.
- webui: Adjusted Bot conversation message table widths so sequence, time, direction, sender, and event columns remain readable while the message column uses the remaining space.

## Compatibility

- legacy-plugin: Legacy E-language image text code `[pic,file=...]` is now converted at the QQBot send layer for local image delivery; QQBot embedded formats such as `<@id>`, `<@!id>`, and `@everyone` are preserved as raw content for Tencent's server to handle.
- legacy-plugin: Group `Api_SendMsg_v2(Type=4)` `ADD/users_openid` is not sent unchanged as an unsupported QQBot field and no longer generates an automatic mention.
- legacy-plugin: Markdown messages produced by `Api.取_消息_MD(...)` are now converted to QQBot `markdown`/`keyboard` request bodies; guild, DMS, group, and C2C sends treat an empty `msg_id` as an active message instead of reporting a missing `msg_id`.
