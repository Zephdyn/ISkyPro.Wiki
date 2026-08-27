# 2.1.0-preview.2 SDK

Status: in development, not yet released

This version prepares for multiple platforms and accounts: message targets can name an explicit account, and adding a platform requires zero SDK or protocol changes.

## ✨ Features

- messages: Message targets accept an optional `botAccountId` (format `"{platform}:{accountId}"`). The event reference of `messages.reply` and the `target` of `messages.send` / `messages.recall` may name an explicit account; absent falls back to the platform default, existing plugins keep their behavior and need no changes.
- messages: Event references automatically carry the source account; replies without an explicit target default back to it.
- errors: Unknown platforms or missing accounts now return stable error codes `message.target.platform_not_supported` / `message.target.account_not_found`.
- api: All four SDKs (C# / Node.js / Python / Go) expose optional account parameters on their message APIs as groundwork for signing in multiple platforms and accounts at once.

## ♻️ Compatibility

- All new protocol fields are optional and backward compatible; already-compiled plugins keep working.
- Adding a platform (WeChat, Feishu, DingTalk, Telegram, Discord, ...) only requires registering a gateway and adapters in Main — SDKs and the protocol stay unchanged.