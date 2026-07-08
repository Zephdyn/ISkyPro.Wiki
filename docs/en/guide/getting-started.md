# Getting Started

ISkyPro is a .NET 10 rewrite of the main framework for QQBot and the legacy ISky plugin ecosystem. It includes the main process, Web management UI, QQBot gateway, isolated legacy DLL plugin host, `message.dll` compatibility layer, and Plugin SDK v2.

The current version is `2.0.0-preview.3`. Preview builds prioritize compatibility for the legacy plugin ABI, the x86 plugin-host packaging model, and major WebUI workflows. The new Plugin SDK v2 remains a preview API.

## Requirements

- Windows x64 / Windows ARM64, or glibc Linux x64 preview
- An ISkyPro release package, such as `ISkyPro-2.0.0-preview.3-win-x64.zip`, `ISkyPro-2.0.0-preview.3-win-arm64.zip`, or `ISkyPro-2.0.0-preview.3-linux-x64.tar.gz`
- Network access to the QQBot platform
- Bot ID / AppID and Secret from your QQ Open Platform bot console

The release package includes runtime components for normal use. Users do not need to install the .NET SDK, Node.js, or compiler toolchains.

Linux preview packages support the main process, WebUI, QQBot gateway, and Plugin SDK v2 modern plugins, and target glibc Linux x64 distributions rather than Alpine/musl. Legacy DLL plugins, the x86 `isky.exe` compatibility host, and the `message.dll` compatibility layer are still only provided in Windows packages.

## Package Layout

After extracting a Windows package, the main entry points and folders are:

```text
ISkyPro/
  ISkyPro.exe              # Main process entry point
  isky.exe                 # Legacy plugin compatibility host, internal component
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

Start only `ISkyPro.exe` manually. `isky.exe` is the legacy plugin compatibility host and is not the user-facing entry point.

After extracting a Linux package, the main entry points and folders are:

```text
ISkyPro/
  ISkyPro                  # Main process entry point
  config/
    appsettings.json
  data/
  plugins-v2/
```

Linux packages do not include the legacy plugin compatibility host, `message.dll`, or Windows Service scripts.

## Start ISkyPro

Windows:

1. Extract the release package to a stable directory, for example `D:\Bots\ISkyPro`.
2. Double-click `ISkyPro.exe`, or run it from a terminal:

```cmd
ISkyPro.exe
```

3. The terminal prints the WebUI address and access token.
4. Open the full address shown in the terminal in your browser.

Linux:

1. Extract the release package to a stable directory, for example `/opt/iskypro`:

```bash
sudo mkdir -p /opt/iskypro
sudo tar -xzf ISkyPro-2.0.0-preview.3-linux-x64.tar.gz -C /opt/iskypro --strip-components=1
sudo chmod +x /opt/iskypro/ISkyPro
```

2. Run it from a terminal:

```bash
cd /opt/iskypro
./ISkyPro
```

3. The terminal prints the WebUI address and access token.
4. Open the full address shown in the terminal in your browser.

The default WebUI address is `http://127.0.0.1:5432`. If the port is already occupied, the terminal shows the actual listening address or the failure reason.

## Log In to QQBot

After opening the WebUI for the first time, go to the Bot login page:

1. Enter the Bot ID / AppID from the QQ Open Platform bot console.
2. Enter the Secret.
3. Select a connection mode.
4. Click verify and save.

Bot ID / AppID and Secret must come from your own QQ bot console. Do not send the Secret to plugin authors or publish it in logs, screenshots, or issues.

## Choose a Connection Mode

Choose by deployment shape:

- Use WebSocket for local use, no public HTTPS callback, or no callback URL configured in QQ Open Platform.
- Use Webhook when you have configured a callback URL in QQ Open Platform or will receive callbacks through public HTTPS / a reverse proxy.
- If QQ Open Platform already has a callback URL, first debug missing messages by checking the Webhook callback URL, signature, and event selections instead of only following the WebSocket path.

The quick start only requires login and mode selection. Group message permissions, event subscriptions, Webhook reverse proxy, and troubleshooting are covered later:

- [QQBot Events](/en/guide/qqbot-events)
- [Webhook and Reverse Proxy](/en/guide/webhook-and-proxy)

## Long-Running Entry Point

For long-running Windows deployments, install a Windows Service. Each instance should use its own directory and service name:

```text
D:\Bots\ISkyPro-A -> ISkyPro-A
D:\Bots\ISkyPro-B -> ISkyPro-B
```

For a single instance, double-click `service-menu.bat` to open the service menu. You can also install and start with the default service name `ISkyPro`:

```cmd
service\install-service.bat start
```

For multiple instances or a custom service name, open an administrator terminal in the instance directory and run:

```cmd
service\install-service.bat ISkyPro-A start
```

Check service status:

```cmd
service\service-status.bat ISkyPro-A
```

Stop and uninstall the service:

```cmd
service\uninstall-service.bat ISkyPro-A
```

For long-running Linux deployments, use systemd. This example assumes the program is under `/opt/iskypro` and runs as user `iskypro`:

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

Save it as `/etc/systemd/system/iskypro.service`, then run:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now iskypro
sudo systemctl status iskypro
```

Service mode does not open a browser automatically. Confirm the WebUI address from `journalctl -u iskypro`, terminal output, or configuration files, then log in with a valid access token.

For multi-instance deployments, each instance must use a different directory, service name, and planned WebUI / Webhook ports.
