# 2.0.0-preview.4

发布日期：2026-07-18

## 🐛 Bug Fixes

- legacy-plugin: 启动扫描和每个插件载入前强制结束本包未托管的残留 `isky.exe`；杀不掉则拒绝启动，避免授权插件双开。temp 唯一副本仅作次要防锁文件手段。
- legacy-plugin: `isky.exe` 启动后尽量抑制 Windows 崩溃对话框，并通过 Job Object 绑定子进程，减少 Server 2012 R2 等环境下崩溃后卡死、旧插件消息无法继续处理的问题。
- message.dll: `Api_GetRunLength` 改为使用 ISkyPro 主程序启动时间；`isky.exe` 崩溃重启后运行时长不再归零。

## ♻️ Compatibility

- legacy-plugin: 旧 DLL 插件的顺序分发、返回值拦截、x86 PluginHost 隔离和拖拽排序保持不变。
- windows: 残留 `isky.exe` 清理与 Job Object 绑定仅作用于 Windows 旧插件宿主路径；Linux preview 包仍不包含 `isky.exe` / `message.dll`。