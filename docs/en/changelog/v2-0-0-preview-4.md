# 2.0.0-preview.4

Release date: 2026-07-22

## 🐛 Bug Fixes

- legacy-plugin: On package scan and before each plugin load, forcibly terminate unmanaged residual `isky.exe` hosts for this package; refuse to start if a residual host cannot be killed, preventing licensed plugins from running twice. Unique temp copies remain only a secondary file-lock defense.
- legacy-plugin: Fix appid validation after unique temp DLL copies so expected id uses the original plugin filename instead of the temp copy stem, preventing false `Plugin id mismatch` load failures.
- legacy-plugin: `isky.exe` now best-effort suppresses Windows crash dialogs and is tracked under a kill-on-close job object, reducing cases where a crash UI on Windows Server 2012 R2 blocks restart and stops legacy plugin message handling.
- message.dll: `Api_GetRunLength` now uses the ISkyPro main-process start time, so host restarts no longer reset framework uptime to zero.

## ♻️ Compatibility

- legacy-plugin: Legacy DLL plugin ordered dispatch, return-value interception, x86 PluginHost isolation, and drag sorting are unchanged.
- windows: Residual `isky.exe` cleanup and job-object tracking apply only to the Windows legacy plugin host path; Linux preview packages still do not include `isky.exe` / `message.dll`.
