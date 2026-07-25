# iskypro-sdk-v2

Official Python runtime for ISkyPro Plugin SDK v2.

```python
from iskypro_sdk_v2 import StdioJsonRpcPlugin, at


def on_event(message, context):
    context.log_write(f"received {message.event_id}")
    message.reply(at.user(message.sender), " hello")
    return {"accepted": True}


StdioJsonRpcPlugin("top.example.plugin").run(on_event)
```

Use `--iskypro-stdio` as the explicit plugin entry argument and reserve stdout
for JSON-RPC frames. Application logs must go to stderr or `context.log_write`.
Use `message.reply(*parts)` for bound replies and
`context.messages.group(...).send(*parts)` for proactive messages.
Use `message.reply_markdown(*parts)` or `send_markdown(*parts)` for QQ group
Markdown messages. The runtime keeps mentions structured and Main sends
`msg_type = 2` with `markdown.content`.

## Package a stdio plugin

The runnable sample contains a standard-library-only packager:

```powershell
Set-Location samples\stdio-python-plugin
python package.py
```

It writes an installable ZIP under `artifacts/` and vendors
`iskypro_sdk_v2` beside the plugin entry point. Copy the sample packager into a
new project or implement the same layout in your own release pipeline.
