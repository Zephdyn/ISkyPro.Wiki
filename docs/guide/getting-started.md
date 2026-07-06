# 快速开始

ISkyPro 是面向 QQBot 和旧 ISky 插件生态的 .NET 10 重构版主框架。它把主框架、Bot 网关、Web 管理界面、旧 DLL 插件隔离宿主、`message.dll` 兼容层和现代 C# 插件 SDK 放在同一个解决方案中。

当前版本为 `2.0.0-preview.2`。预览版会优先保持旧插件 ABI、x86 插件宿主发布策略和主要 WebUI/API 行为稳定；`ISkyPro.Contracts` 与 `ISkyPro.PluginSdk` 在稳定版前仍可能调整。

## 环境要求

- Windows
- ISkyPro 发布包
- 可访问 QQBot 平台的网络环境
- 如需长期运行，需要管理员权限安装 Windows Service

发布包自带运行所需组件，普通用户不需要安装 .NET SDK、Node.js 或编译工具链。

## 发布包结构

解压发布包后，主要入口和目录如下：

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

## 手动启动

1. 解压发布包到固定目录，例如 `D:\Bots\ISkyPro`。
2. 双击 `ISkyPro.exe`，或在命令行运行：

```cmd
ISkyPro.exe
```

3. 终端会输出 WebUI 地址和访问 token。
4. 在浏览器打开终端输出的完整地址进入 WebUI。

默认 WebUI 地址为 `http://127.0.0.1:5432`。如果端口被占用，请先关闭占用程序，或在配置中调整监听地址。

## 长期运行

长期运行建议安装 Windows Service。每个实例使用一个独立目录和一个独立服务名：

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

多实例部署时，每个实例必须使用不同目录、不同服务名，并规划不同的 WebUI / Webhook 端口。
