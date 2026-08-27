# SDK Downloads

The canonical Plugin SDK v2 source is under `sdk/` in this public repository. You
can clone it directly or download a language archive from the matching Release.
The latest stable SDK is `2.0.0`; `2.1.0-preview.1` is the latest preview with no
stable naming freeze.

Download entries:

- Latest preview: [v2.1.0-preview.1 Release](https://github.com/Zephdyn/ISkyPro.Wiki/releases/tag/v2.1.0-preview.1)
- Latest stable: [v2.0.0 Release](https://github.com/Zephdyn/ISkyPro.Wiki/releases/tag/v2.0.0)

Main application package names:

| Platform | File name |
| --- | --- |
| Windows x64 | `ISkyPro-2.1.0-preview.1-win-x64.zip` |
| Windows ARM64 | `ISkyPro-2.1.0-preview.1-win-arm64.zip` |
| Linux x64 | `ISkyPro-2.1.0-preview.1-linux-x64.tar.gz` |

SDK archive names:

| SDK | File name |
| --- | --- |
| ISky v1 SDK (*EPL*) | `SDK-V1-EPL-2.1.0-preview.1.zip` |
| C# Plugin SDK v2 | `SDK-V2-CSharp-2.1.0-preview.1.zip` |
| Python Plugin SDK v2 | `SDK-V2-Python-2.1.0-preview.1.zip` |
| Node.js Plugin SDK v2 | `SDK-V2-NodeJS-2.1.0-preview.1.zip` |
| Go Plugin SDK v2 | `SDK-V2-Go-2.1.0-preview.1.zip` |

SDK archives are for plugin development, not application installation or updates.
The C# archive includes NuGet packages, public contracts, SDK source, and a runnable
sample. Other archives preserve the relative `sdk/<language>` and `samples/<sample>`
layout so their samples build immediately after extraction.
