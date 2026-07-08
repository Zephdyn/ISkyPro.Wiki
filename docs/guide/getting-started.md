# 快速开始

ISkyPro 是面向 QQBot 和旧 ISky 插件生态的 .NET 10 重构版主框架。它包含主程序、Web 管理界面、QQBot 网关、旧 DLL 插件隔离宿主、`message.dll` 兼容层和 Plugin SDK v2。

当前版本为 `2.0.0-preview.3`。预览版会优先保持旧插件 ABI、x86 插件宿主发布策略和主要 WebUI 行为稳定；新的 Plugin SDK v2 仍是 preview API。

## 准备

- Windows x64 / Windows ARM64，或 glibc Linux x64 preview
- ISkyPro 发布包，例如 `ISkyPro-2.0.0-preview.3-win-x64.zip`、`ISkyPro-2.0.0-preview.3-win-arm64.zip` 或 `ISkyPro-2.0.0-preview.3-linux-x64.tar.gz`
- 可访问 QQBot 平台的网络环境
- QQ 开放平台机器人管理后台中的 Bot ID / AppID 和 Secret

发布包自带运行所需组件，普通用户不需要安装 .NET SDK、Node.js 或编译工具链。

Linux preview 包支持主程序、WebUI、QQBot 网关和 Plugin SDK v2 新插件，目标为 glibc Linux x64 发行版，不面向 Alpine/musl。旧 DLL 插件、`isky.exe` x86 兼容宿主和 `message.dll` 兼容层仍只在 Windows 包中提供。

## 发布包结构

Windows 包解压后，主要入口和目录如下：

```text
ISkyPro/
  ISkyPro.exe              # 主程序入口
  isky.exe                 # 旧插件兼容宿主，内部组件
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

手动运行时只启动 `ISkyPro.exe`。`isky.exe` 是旧插件兼容宿主，不是用户启动入口。

Linux 包解压后，主要入口和目录如下：

```text
ISkyPro/
  ISkyPro                  # 主程序入口
  config/
    appsettings.json
  data/
  plugins-v2/
```

Linux 包不包含旧插件兼容宿主、`message.dll` 或 Windows Service 脚本。

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
sudo tar -xzf ISkyPro-2.0.0-preview.3-linux-x64.tar.gz -C /opt/iskypro --strip-components=1
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
