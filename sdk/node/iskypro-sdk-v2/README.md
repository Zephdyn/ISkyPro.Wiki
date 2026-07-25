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
