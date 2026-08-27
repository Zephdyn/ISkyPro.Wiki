# 2.1.0-preview.1 SDK

Release date: 2026-08-25

This release brings rich media and group management to the Plugin SDK v2: local and remote image sending, message recall, and low-level catalog methods that now forward to the real QQBot OpenAPI.

## ✨ Features

- messages: Added image sending to structured messages. The C#, Python, Node.js, and Go SDKs provide `Image.FromFile`, `image()`, `image(filePath)`, and `Image(filePath)` respectively; pass a local image path on the machine running ISkyPro to send a group or private-chat image (rich media `msg_type = 7`). A message allows at most one image and cannot be combined with Markdown; send additional images as separate messages.
- messages: Added a remote-image `image-url` part (`Image.FromUrl`, `image_url`, `imageUrl`, `ImageUrl`). When the image has a public URL, Main uses the official URL upload flow (`file_type=1` + `url` + `srv_send_msg=false` → `file_info` → rich media), supported for group and C2C targets only. The local base64 `file_data` upload path stays as a compatibility capability and is not migrated.
- messages: Added message recall. The four SDKs expose `RecallAsync` / `recall()` / `recall(messageId)` / `Recall()` to recall messages the bot sent itself (QQ: messages older than 2 minutes cannot be recalled; text-channel/direct-message recall is private-domain only). The manifest must declare the `messages.recall` permission.
- protocol: The messaging path is now platform-neutral. `messageReference` and the `messages.send` target support an optional `platform` field; already-compiled C# plugins replying inside OneBot events are routed through the plugin's last event platform automatically.
- catalog: Low-level catalog methods now forward to the real QQBot OpenAPI instead of returning placeholders. `unsafe.rawOpenApi` remains disabled by default, but can invoke arbitrary QQBot OpenAPI endpoints when the global allow switch is enabled and the plugin declares the permission.
- errors: Structured QQ platform errors. Failures now return JSON-RPC error `data` with `errorCode` (`qq.api.<errCode>` or `qq.api.http.<status>`), `statusCode`, and `platformErrorCode`, so errors such as the 22009 message rate limit can be handled by code.
- events: Event attachments and mentions are now populated automatically. QQ media messages (image/video/voice/file) appear in `message.attachments` (`url`/`content_type`/`size`), and mentioned users in guild messages appear in `message.mentions`; OneBot voice/video/file segments are mapped the same way.
- catalog: Added group-management methods to the low-level catalog: bot state (`groups.getBotState`), group mute query/setup, join-request list and approval, and join-auto-approval strategies (list/create/update/delete/execute/whitelist). `groups.getGroupInfo` now uses the official path (the previous path failed).

## ♻️ Compatibility

- For images, prefer the high-level `messages.send` / `messages.reply` `image` (local file) or `image-url` (remote URL) parts; low-level catalog methods (`media.uploadC2CFile` and others) do forward to the real API but are not recommended for direct use.
- Proactive messaging follows the current QQ official rules: enterprise-, personal-, and unverified-tier bots can all send proactive messages, with different qps/qpm and daily quotas; users can disable "allow proactive messages" in the QQ client, after which sends fail (plugins receive structured errors such as `qq.api.22009`).