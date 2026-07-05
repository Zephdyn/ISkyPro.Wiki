# 快速开始

ISkyPro 是面向 QQBot 和旧 ISky 插件生态的 .NET 10 重构版主框架。它把主框架、Bot 网关、Web 管理界面、旧 DLL 插件隔离宿主、`message.dll` 兼容层和现代 C# 插件 SDK 放在同一个解决方案中。

当前版本为 `2.0.0-preview.1`。预览版会优先保持旧插件 ABI、x86 插件宿主发布策略和主要 WebUI/API 行为稳定；`ISkyPro.Contracts` 与 `ISkyPro.PluginSdk` 在稳定版前仍可能调整。

## 环境要求

- Windows
- .NET SDK 10.0.301 或兼容 feature band roll-forward 的版本
- Windows PowerShell
- Node.js 和 pnpm，用于构建 WebUI
- 32-bit `g++` 工具链，用于构建原生 `message.dll`

## 本地启动

推荐从主仓库根目录运行：

```powershell
.\iskypro.ps1 debug
```

也可以直接运行主框架：

```powershell
dotnet run --project src\ISkyPro.Main\ISkyPro.Main.csproj
```

默认 WebUI 地址为：

```text
http://127.0.0.1:5432
```

启动后终端会输出带访问 token 的完整 WebUI 地址。复制完整地址可以自动写入访问 token 并进入后台。

## Wiki 本地预览

主仓库挂载 `wiki/` submodule 后，可以进入该目录预览文档站点：

```powershell
Set-Location wiki
pnpm install
pnpm docs:dev
```

生产构建：

```powershell
pnpm docs:build
```
