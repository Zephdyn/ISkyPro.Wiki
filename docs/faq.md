# FAQ

## WebUI token 在哪里看？

首次启动 `ISkyPro.exe` 或 Linux 包中的 `ISkyPro` 时，终端会输出 WebUI 地址和访问 token。服务模式下不会自动弹浏览器，请查看服务日志或启动输出。

## 应该选 WebSocket 还是 Webhook？

本机运行、没有公网 HTTPS 回调地址时选 WebSocket。已经在 QQ 平台配置公网回调地址，或部署目标就是公网 HTTPS 回调时选 Webhook。

## 为什么登录成功但收不到群消息？

常见原因是群全量消息开关未开、Webhook 没勾选 `GROUP_MESSAGE_CREATE`、连接模式与 QQ 平台配置不一致，或插件没有处理该事件。

## Bot ID / AppID 和 Secret 在哪里获取？

在 QQ 开放平台机器人管理后台获取。不要从第三方插件、文档截图或别人发来的配置中复制 Secret。

## Webhook 回调地址应该填什么？

填写公网 HTTPS 地址，路径要与 ISkyPro Webhook 设置一致，例如 `https://bot.example.com/qqbot/webhook`。

## 旧插件和新插件有什么区别？

旧插件是 DLL 插件，放在 `plugin/`，由 x86 兼容宿主运行。新插件使用 Plugin SDK v2，放在 `plugins-v2/`，通过 manifest 和 stdio/HTTP 协议接入。

## Linux 支持旧插件吗？

不支持。Linux preview 包支持主程序、WebUI、QQBot 网关和 Plugin SDK v2 新插件；旧 DLL 插件依赖 Windows/x86 兼容宿主和 `message.dll`，仍需要 Windows 包。

## 为什么旧插件需要 Windows/x86 兼容宿主？

旧生态插件依赖历史 DLL ABI 和 32 位运行环境。ISkyPro 用独立 x86 宿主隔离它们，避免直接加载到主程序进程。

## 新插件 zip 上传后为什么没有启动？

安装成功不一定自动启动。确认上传时勾选了“安装后立即启动”，或在新插件页手动启动。运行中的插件更新前需要先停止。

## 更新检测失败是否影响运行？

不影响。更新检测失败只影响 WebUI 关于页的新版提示和下载入口，Bot 和插件运行不依赖它。

## 服务模式下为什么不会自动弹浏览器？

Windows Service 或 Linux systemd 都在后台会话中运行，不能像桌面程序一样弹出浏览器。请手动打开 WebUI 地址并输入访问 token。
