# @iskypro/plugin-sdk-v2

Official Node.js runtime for ISkyPro Plugin SDK v2.

```js
import { StdioJsonRpcPlugin, at } from "@iskypro/plugin-sdk-v2";

await new StdioJsonRpcPlugin("top.example.plugin").run(async (message, context) => {
  await context.logWrite(`received ${message.eventId}`);
  await message.reply(at.user(message.sender), " hello");
  return { accepted: true };
});
```

Use `--iskypro-stdio` as the explicit plugin entry argument and reserve stdout
for JSON-RPC frames. Application logs must go to stderr or `context.logWrite`.
Use `context.messages.group(...).send(...parts)` for proactive messages.
Use `message.replyMarkdown(...parts)` or `sendMarkdown(...parts)` for QQ group
Markdown messages. Mentions remain structured and Main sends `msg_type = 2`
with `markdown.content`.

## Package a stdio plugin

The runnable sample provides a dependency-free ZIP command:

```powershell
Set-Location samples\stdio-node-plugin
npm run package:plugin
```

The command vendors `@iskypro/plugin-sdk-v2` under `node_modules` and writes the
installable ZIP under `artifacts/`. `node package-plugin.mjs` is the equivalent
command when npm scripts are not being used.
