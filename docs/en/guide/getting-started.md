# Getting Started

ISkyPro is a .NET 10 rewrite of the QQBot main framework, built for compatibility with and extension of the ISky plugin ecosystem. It consists of the main process, Web management UI, QQBot gateway, isolated ISky v1 *EPL* / x86 plugin host, `message.dll` compatibility layer, and the ISkyPro v2+ Plugin SDK.

The latest stable version is `2.0.0`, and the latest released preview is `2.1.0-preview.1`. Plugin SDK v2 provides public sources for C#, Python, Node.js, and Go.

## Requirements

- Windows x64 / Windows ARM64, or glibc Linux x64
- An ISkyPro release package, such as `ISkyPro-2.0.0-win-x64.zip`, `ISkyPro-2.0.0-win-arm64.zip`, or `ISkyPro-2.0.0-linux-x64.tar.gz`
- Network access to the QQBot platform
- Bot ID / AppID and Secret from your QQ Open Platform bot console

The release package includes runtime components for normal use. Users do not need to install the .NET SDK, Node.js, or compiler toolchains.

Linux x64 packages support the main process, WebUI, QQBot gateway, and ISkyPro v2+ plugins, and target glibc Linux x64 distributions rather than Alpine/musl. ISky v1 plugins, the x86 `isky.exe` compatibility host, and the `message.dll` compatibility layer are still only provided in Windows packages.

## Package Layout

After extracting a Windows package, the main entry points and folders are:

```text
ISkyPro/
  ISkyPro.exe              # Main process entry point
  isky.exe                 # ISky v1 EPL/x86 compatibility host, internal component
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

Start only `ISkyPro.exe` manually. `isky.exe` is the v1 plugin compatibility host and is not the user-facing entry point.

After extracting a Linux package, the main entry points and folders are:

```text
ISkyPro/
  ISkyPro                  # Main process entry point
  config/
    appsettings.json
  data/
  plugins-v2/
```

Linux packages do not include the v1 plugin compatibility host, `message.dll`, or Windows Service scripts.

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
sudo tar -xzf ISkyPro-2.0.0-linux-x64.tar.gz -C /opt/iskypro --strip-components=1
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

## Connecting the OneBot Platform

> The following describes 2.1.0 preview behavior and may change before the stable release.

Since 2.1.0, ISkyPro supports a OneBot platform gateway that can run alongside
other OneBot-compatible clients/protocol implementations. The OneBot and QQBot
gateways are independent of each other:

- Forward WebSocket: ISkyPro actively connects to the OneBot protocol endpoint.
- Reverse WebSocket: the OneBot protocol endpoint connects to ISkyPro's
  `/onebot/ws` endpoint, with optional `access_token` validation.
- Message protocols support OneBot v11 and v12; the send layer supports CQ-code
  rich media such as images, mentions, replies, and voice, with outbound throttling.

Select the "OneBot" platform on the WebUI login page, or enable the `onebot`
section in the configuration file. Multi-platform state is visible on the gateway
page. Configuration example (`config/appsettings.json`):

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

`ProtocolVersion` accepts `V11` / `V12`; `ConnectionMode` accepts
`ForwardWebSocket` / `ReverseWebSocket` / `Http`. For reverse WebSocket and HTTP
modes, fill in `ApiBaseUrl` and `SelfUserId` as required by the protocol
endpoint, and keep `AccessToken` consistent with that endpoint.

## Configuration Overrides

Besides the WebUI and `config/appsettings.json`, environment variables and
command-line arguments override configuration files (higher precedence):

- Environment variables: `ISkyPro:WebUI:AllowRemote=true`. On platforms whose
  environment variable names cannot contain colons (such as Windows), use the
  double-underscore form `ISkyPro__WebUI__AllowRemote=true`.
- Command line: `ISkyPro.exe --ISkyPro:WebUI:AllowRemote=true` (or
  `--ISkyPro__WebUI__AllowRemote=true`).
- Dedicated flags: `--urls` (Kestrel listen address), `--open-browser` /
  `--no-open-browser` (control automatic browser opening), and `--service-name`
  (service name, used with the install scripts).

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
