# iskypro-sdk-v2

Official Python runtime for ISkyPro Plugin SDK v2.

```python
from iskypro_sdk_v2 import StdioJsonRpcPlugin


def on_event(event, context):
    context.log_write(f"received {event.get('eventId', '')}")
    return {"accepted": True}


StdioJsonRpcPlugin("top.example.plugin").run(on_event)
```

Use `--iskypro-stdio` as the explicit plugin entry argument and reserve stdout
for JSON-RPC frames. Application logs must go to stderr or `context.log_write`.
