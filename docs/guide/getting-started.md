# 快速开始

ISkyPro 是基于 .NET 10 重写的 QQBot 主框架，面向 ISky 插件生态提供兼容与扩展。它由主程序、Web 管理界面、QQBot 网关、ISky v1 插件兼容宿主、`message.dll` 兼容层和 ISkyPro v2+ Plugin SDK 组成。

最新稳定版本为 `2.0.0`，最新已发布预览版为 `2.1.0-preview.2`。Plugin SDK v2 已提供 C#、Python、Node.js 和 Go 公共源码。

## 准备

- Windows x64 / Windows ARM64，或 glibc Linux x64
- ISkyPro 发布包，例如 `ISkyPro-2.0.0-win-x64.zip`、`ISkyPro-2.0.0-win-arm64.zip` 或 `ISkyPro-2.0.0-linux-x64.tar.gz`
- 可访问 QQBot 平台的网络环境
- QQ 开放平台机器人管理后台中的 Bot ID / AppID 和 Secret

发布包自带运行所需组件，普通用户不需要安装 .NET SDK、Node.js 或编译工具链。

Linux x64 包支持主程序、WebUI、QQBot 网关和 ISkyPro v2+ 插件，目标为 glibc Linux x64 发行版，不面向 Alpine/musl。ISky v1 插件、`isky.exe` x86 兼容宿主和 `message.dll` 兼容层仍只在 Windows 包中提供。

## 发布包结构

Windows 包解压后，主要入口和目录如下：

```text
ISkyPro/
  ISkyPro.exe              # 主程序入口
  isky.exe                 # ISky v1 EPL/x86 兼容宿主，内部组件
  bin/
    message.dll
  config/
    appsettings.json
  data/
  plugin/
  service-menu.bat
  service/
    install-service.bat
    uninstall-service.bat
    service-status.bat
```

手动运行时只启动 `ISkyPro.exe`。`isky.exe` 是 ISky v1 插件兼容宿主，不是用户启动入口。

Linux 包解压后，主要入口和目录如下：

```text
ISkyPro/
  ISkyPro                  # 主程序入口
  config/
    appsettings.json
  data/
  plugins-v2/
```

Linux 包不包含 v1 插件兼容宿主、`message.dll` 或 Windows Service 脚本。

## 启动 ISkyPro

Windows：

1. 解压发布包到固定目录，例如 `D:\Bots\ISkyPro`。
2. 双击 `ISkyPro.exe`，或在命令行运行：

```cmd
ISkyPro.exe
```

3. 终端会输出 WebUI 地址和访问 token。
4. 在浏览器打开终端输出的完整地址进入 WebUI。

Linux：

1. 解压发布包到固定目录，例如 `/opt/iskypro`：

```bash
sudo mkdir -p /opt/iskypro
sudo tar -xzf ISkyPro-2.0.0-linux-x64.tar.gz -C /opt/iskypro --strip-components=1
sudo chmod +x /opt/iskypro/ISkyPro
```

2. 在终端运行：

```bash
cd /opt/iskypro
./ISkyPro
```

3. 终端会输出 WebUI 地址和访问 token。
4. 在浏览器打开终端输出的完整地址进入 WebUI。

默认 WebUI 地址为 `http://127.0.0.1:5432`。如果端口被占用，终端会提示实际监听地址或失败原因。

## 登录 QQBot

首次进入 WebUI 后，打开 Bot 登录页面：

1. 填写 QQ 开放平台机器人管理后台中的 Bot ID / AppID。
2. 填写 Secret。
3. 选择连接模式。
4. 点击验证并保存。

Bot ID / AppID 和 Secret 只来自你自己的 QQ 机器人后台。不要把 Secret 发给插件作者或公开在日志、截图、Issue 中。

## 选择连接模式

优先按部署方式选择：

- 本机使用、没有公网 HTTPS 回调、QQ 开放平台未配置回调地址时，选择 WebSocket。
- 已经在 QQ 开放平台配置回调地址，或准备通过公网 HTTPS / 反向代理接收回调时，选择 Webhook。
- 如果 QQ 开放平台已经配置了回调地址，首次排查收不到消息时先按 Webhook 的回调地址、签名和事件勾选排查，不要只按 WebSocket 路线排查。

快速开始只需要完成登录和模式选择。群消息权限、事件订阅、Webhook 反代和排障见后续页面：

- [QQBot 事件配置](/guide/qqbot-events)
- [Webhook 与反向代理](/guide/webhook-and-proxy)

## 接入 OneBot 平台

> 以下为 2.1.0 预览版特性，正式发布前行为可能调整。

2.1.0 起，ISkyPro 支持 OneBot 平台网关，可与其他 OneBot 兼容客户端/协议端并行运行。
OneBot 网关与 QQBot 网关相互独立：

- 正向 WebSocket：由 ISkyPro 主动连接 OneBot 协议端地址。
- 反向 WebSocket：OneBot 协议端连接 ISkyPro 的 `/onebot/ws` 接入点，可配置
  `access_token` 校验。
- 消息协议支持 OneBot v11 与 v12；发送层支持图片、@、回复、语音等 CQ 码富媒体，并带出站限速。

在 WebUI 登录页选择“OneBot”平台，或直接编辑配置文件中的 `onebot` 节启用。多平台状态可在
网关页查看。配置文件示例（`config/appsettings.json`）：

```json
{
  "ISkyPro": {
    "OneBot": {
      "Enabled": true,
      "ProtocolVersion": "V11",
      "ConnectionMode": "ForwardWebSocket",
      "WsUrl": "ws://127.0.0.1:8080",
      "ApiBaseUrl": "http://127.0.0.1:3000",
      "AccessToken": "",
      "SelfUserId": 0,
      "DisplayName": "OneBot",
      "ReverseWebSocketPath": "/onebot/ws",
      "MaxReconnectBackoffSeconds": 60
    }
  }
}
```

`ProtocolVersion` 可填 `V11` / `V12`；`ConnectionMode` 可填 `ForwardWebSocket` / `ReverseWebSocket` / `Http`。
反向 WebSocket 与 HTTP 模式时，`ApiBaseUrl` 与 `SelfUserId` 按协议端要求填写；`AccessToken` 与协议端配置保持一致。

## 配置覆盖方式

除 WebUI 与 `config/appsettings.json` 外，还支持环境变量和命令行参数覆盖，优先级高于配置文件：

- 环境变量：`ISkyPro:WebUI:AllowRemote=true`。环境变量名不支持冒号的平台
  （如 Windows）可用双下划线形式 `ISkyPro__WebUI__AllowRemote=true`。
- 命令行：`ISkyPro.exe --ISkyPro:WebUI:AllowRemote=true`（或
  `--ISkyPro__WebUI__AllowRemote=true`）。
- 特殊参数：`--urls`（Kestrel 监听地址）、`--open-browser` / `--no-open-browser`
  （是否自动打开浏览器）、`--service-name`（服务名，与安装脚本配合）。

## 长期运行入口

Windows 长期运行建议安装 Windows Service。每个实例使用一个独立目录和一个独立服务名：

```text
D:\Bots\ISkyPro-A -> ISkyPro-A
D:\Bots\ISkyPro-B -> ISkyPro-B
```

单实例可以直接双击 `service-menu.bat` 打开服务管理菜单。也可以用默认服务名
`ISkyPro` 直接安装并启动：

```cmd
service\install-service.bat start
```

多实例或自定义服务名时，以管理员身份打开命令行，在实例目录运行：

```cmd
service\install-service.bat ISkyPro-A start
```

查看服务状态：

```cmd
service\service-status.bat ISkyPro-A
```

停止并卸载服务：

```cmd
service\uninstall-service.bat ISkyPro-A
```

Linux 长期运行建议使用 systemd。下面示例假设程序位于 `/opt/iskypro`，运行用户为 `iskypro`：

```ini
[Unit]
Description=ISkyPro
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
User=iskypro
WorkingDirectory=/opt/iskypro
ExecStart=/opt/iskypro/ISkyPro --no-open-browser
Restart=on-failure
RestartSec=5

[Install]
WantedBy=multi-user.target
```

保存为 `/etc/systemd/system/iskypro.service` 后执行：

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now iskypro
sudo systemctl status iskypro
```

服务模式不会自动打开浏览器。请从 `journalctl -u iskypro`、终端输出或配置文件确认 WebUI 地址，并使用有效访问 token 登录。

多实例部署时，每个实例必须使用不同目录、不同服务名，并规划不同的 WebUI / Webhook 端口。
