# @iskypro/plugin-sdk-v2

Official Node.js runtime for ISkyPro Plugin SDK v2.

```js
import { StdioJsonRpcPlugin } from "@iskypro/plugin-sdk-v2";

await new StdioJsonRpcPlugin("top.example.plugin").run(async (event, context) => {
  await context.logWrite(`received ${event.eventId}`);
  return { accepted: true };
});
```

Use `--iskypro-stdio` as the explicit plugin entry argument and reserve stdout
for JSON-RPC frames. Application logs must go to stderr or `context.logWrite`.
