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
