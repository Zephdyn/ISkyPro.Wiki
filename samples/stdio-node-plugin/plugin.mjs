import { StdioJsonRpcPlugin } from "../../sdk/node/iskypro-sdk-v2/index.js";

const pluginId = "top.iskypro.sample.node-stdio";

if (!process.argv.includes("--iskypro-stdio")) {
  console.error("This plugin is meant to be run by ISkyPro with --iskypro-stdio.");
  process.exit(2);
}

async function onEvent(event, context) {
  const eventId = event.eventId ?? "";
  const content = event.message?.content ?? "";
  const reference = event.messageReference ?? {};
  console.error(`node sample received ${eventId}`);
  await context.logWrite(`node sample handled ${eventId}`);
  if (content.length > 0) {
    await context.replyText(reference, `node echo: ${content}`);
  }

  return { accepted: true };
}

await new StdioJsonRpcPlugin(pluginId, "iskypro-node-sdk-v2").run(onEvent);
