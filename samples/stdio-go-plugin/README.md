# Go stdio Plugin Sample

Build the current platform binary and create an installable plugin ZIP:

```powershell
go run ./tools/package-plugin
```

Cross-compile when needed:

```powershell
go run ./tools/package-plugin -goos linux -goarch amd64
go run ./tools/package-plugin -goos windows -goarch arm64
go run ./tools/package-plugin -goos darwin -goarch arm64
```

Output files are written under `artifacts/`. The packaging tool builds a native
binary and rewrites the packaged manifest to launch that binary, so the target
machine does not need the Go toolchain.
