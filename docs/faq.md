# FAQ

## WebUI token 在哪里看？

首次启动 `ISkyPro.exe` 或 Linux 包中的 `ISkyPro` 时，终端会输出 WebUI 地址和访问 token。服务模式下不会自动弹浏览器，请查看服务日志或启动输出。

## 应该选 WebSocket 还是 Webhook？

本机运行、没有公网 HTTPS 回调地址时选 WebSocket。已经在 QQ 平台配置公网回调地址，或部署目标就是公网 HTTPS 回调时选 Webhook。详细配置与排障见 [QQBot 事件配置](/guide/qqbot-events) 和 [Webhook 与反向代理](/guide/webhook-and-proxy)。

## 为什么登录成功但收不到群消息？

常见原因是群全量消息开关未开、Webhook 没勾选 `GROUP_MESSAGE_CREATE`、连接模式与 QQ 平台配置不一致，或插件没有处理该事件。

## Bot ID / AppID 和 Secret 在哪里获取？

在 QQ 开放平台机器人管理后台获取。不要从第三方插件、文档截图或转发的配置中复制 Secret。

## Webhook 回调地址应该填什么？

填写公网 HTTPS 地址，路径要与 ISkyPro Webhook 设置一致，例如 `https://bot.example.com/qqbot/webhook`。

## ISky v1 插件和 ISkyPro v2+ 插件有什么区别？

ISky v1 插件是兼容原 ISky 框架 v1 ABI 的 *EPL* / x86 DLL 插件，放在 `plugin/`，由 x86 兼容宿主运行。ISkyPro v2+ 插件使用 Plugin SDK v2，通过 manifest 和 stdio-jsonrpc / HTTP transport 接入；本地 stdio 插件安装在 `plugins-v2/`。

## Linux 支持 ISky v1 插件吗？

不支持。Linux x64 包支持主程序、WebUI、QQBot 网关和 ISkyPro v2+ 插件；ISky v1 插件依赖 Windows/x86 兼容宿主和 `message.dll`，仍需要 Windows 包。

## 为什么 ISky v1 插件需要 Windows/x86 兼容宿主？

ISky v1 插件依赖原 ISky 框架 DLL ABI 和 32 位运行环境。ISkyPro 用独立 x86 宿主隔离它们，避免直接加载到主程序进程。

## ISkyPro v2+ 插件 ZIP 上传后为什么没有启动？

安装成功不一定自动启动。确认上传时勾选了“安装后立即启动”，或在插件列表页手动启动。更新运行中的插件时，框架会先停止旧版本并在替换完成后自动恢复运行，无需手动停止。

## 更新检测失败是否影响运行？

不影响。更新检测失败只影响 WebUI 关于页的新版提示和下载入口，Bot 和插件运行不依赖它。

## 服务模式下为什么不会自动弹浏览器？

Windows Service 或 Linux systemd 都在后台会话中运行，不能像桌面程序一样弹出浏览器。请手动打开 WebUI 地址并输入访问 token。

## 如何从其他机器访问 WebUI？

默认 WebUI 只监听本机回环地址。远程访问需要同时满足两个条件：

1. 将 `ISkyPro:WebUI:Url` 改为可路由的监听地址，例如 `http://0.0.0.0:5432`。
2. 开启远程访问：将 `ISkyPro:WebUI:AllowRemote` 设为 `true`，或在 WebUI 设置中开启远程访问。

修改后重启 ISkyPro，通过 `http://<服务器地址>:5432` 访问并输入启动时输出的访问 token。远程访问务必放在 HTTPS 反向代理之后，不要把 WebUI 登录页直接暴露到公网。

## 如何升级 ISkyPro？

较低版本可通过 WebUI 关于页的更新入口检测并下载新版；也可以手动下载新版发布包，替换旧文件时保留 `config`、`data` 和插件目录。升级前查看 [更新日志](/changelog/) 中的兼容性说明。SDK 请按 `SDK-V2-<Language>-<version>.zip` 单独下载，SDK 压缩包不用于安装或更新主程序。
