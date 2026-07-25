# Node.js stdio Plugin Sample

Create an installable plugin ZIP:

```powershell
npm run package:plugin
```

If npm is unavailable, the underlying command is:

```powershell
node package-plugin.mjs
```

Output:

```text
artifacts/top.iskypro.sample.node-stdio.zip
```

The package script uses only Node.js built-ins and vendors
`@iskypro/plugin-sdk-v2` under `node_modules`. The target machine must provide a
compatible `node` command. It uses a locally installed SDK package when present,
then falls back to the public repository checkout. Set `ISKYPRO_NODE_SDK_PATH`
to select a specific SDK package directory.
