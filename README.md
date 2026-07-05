# ISkyPro Wiki

ISkyPro Wiki is the public documentation site for ISkyPro. It is built with
VitePress and is intended to be managed as an independent Git repository mounted
into the main ISkyPro repository as the `wiki/` submodule.

## Commands

```powershell
pnpm install
pnpm docs:dev
pnpm docs:build
pnpm docs:preview
```

On Windows paths containing `#`, `pnpm docs:dev` starts from a temporary
hash-free workspace because Vite dev-server file URLs treat `#` as a URL
fragment. Restart the dev server after editing docs so the temporary copy is
refreshed.

## Release Notes

Use GitHub Releases as the update-check source for ISkyPro clients:

- Tag format: `v2.0.0-preview.1`
- Release title: `ISkyPro 2.0.0-preview.1`
- Release body: concise changelog summary for WebUI preview
- Full details: link to the matching page under `/changelog/`

For GitHub Pages repository sites, set `VITEPRESS_BASE` to the repository path
when building, for example `/ISkyPro.Wiki/`.
