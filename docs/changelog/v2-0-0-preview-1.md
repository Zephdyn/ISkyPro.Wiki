# 2.0.0-preview.1

发布日期：2026-07-05

## Breaking Changes

- runtime: 主程序迁移到 .NET 10 和 ASP.NET Core，启动、发布和部署方式与旧版 ISky 主程序不同。
- package: 主程序按 win-x64 / win-arm64 发布；旧 DLL 插件宿主作为独立 win-x86 进程发布。
- plugin-host: 旧 DLL 插件不再加载到主进程内，改由隔离宿主进程运行并由主程序监督生命周期。
- api: 当前版本为预览版，`ISkyPro.Contracts` 和 `ISkyPro.PluginSdk` 在正式版前仍可能发生兼容性调整。

## Features

- dashboard: 新增运行概览，显示网关状态、消息收发计数、插件数量和最近框架日志。
- bot: 新增 QQBot 验证登录、WebSocket / Webhook 模式选择、Mention 过滤配置和退出登录。
- plugins: 新增旧 DLL 插件扫描、上传、启用、禁用、重启、打开设置和卸载操作。
- plugins: 旧插件宿主请求改为有界 FIFO 串行队列，并在设置中新增分发队列容量，避免并发消息投递直接失败。
- logs: 新增框架日志、插件日志、Bot 会话日志查看，并支持搜索、分页和清理。
- settings: 新增 WebUI 访问控制、Webhook 监听地址、反向代理配置生成、日志保留策略和调试工具。
- about: 新增侧边栏底部“关于”入口，显示当前版本和更新日志。

## Plugin Development

- contracts: 新增 `ISkyPro.Contracts`，集中定义 IPC、Bot 和插件宿主之间共享的合同类型。
- sdk: 新增 `ISkyPro.PluginSdk`，用于开发现代 C# 插件。
- sample: 新增 `ISkyPro.SamplePlugin`，作为现代插件开发示例。
- debug: 新增 WebUI 模拟消息入口，用于验证现代插件分发链路，不会向 QQ 发送消息。
- native: 新增 `message.dll` 兼容层，保留旧插件 ABI 接入路径。
- native: 重构 `message.dll` 内部实现，拆分授权、队列、JSON 载荷、成员查询、统计和返回缓冲等模块，并补充并发、唤醒和成员查询测试。

## Compatibility

- plugin: 保留旧 DLL 插件的启动、停止、重启、启用、禁用、设置窗口和卸载路径。
- plugin: 保留旧插件通过 `message.dll` 进入消息兼容层的调用方式。
- plugin: 同一旧插件 DLL 的生命周期和消息回调继续串行执行；并发入口现在排队或按超时返回，不再触发单 pending request 异常。
- host: 旧插件宿主保持 Windows x86 兼容性。
- native: `message.dll` 导出函数、调用约定和旧插件可见返回语义保持不变。
- state: 插件启用状态、崩溃计数、连续崩溃禁用和重启窗口由主程序集中管理。

## Docs

- changelog: 新增根目录 `CHANGELOG.md` 作为发布记录的单一来源。
- i18n: 新增英文更新日志源 `CHANGELOG.en-US.md`，WebUI 会按当前语言显示对应内容。
- webui: WebUI 构建时读取更新日志，并使用最新条目作为当前版本。
- release: 后续发布需要在更新日志顶部继续新增版本条目。
