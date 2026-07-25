from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "sdk" / "python"))

from iskypro_sdk_v2 import StdioJsonRpcPlugin  # noqa: E402


PLUGIN_ID = "top.iskypro.sample.python-stdio"


def on_event(event, context):
    event_id = event.get("eventId", "")
    content = (event.get("message") or {}).get("content", "")
    reference = event.get("messageReference") or {}
    print(f"python sample received {event_id}", file=sys.stderr)
    context.log_write(f"python sample handled {event_id}")
    if content:
        context.reply_text(reference, f"python echo: {content}")
    return {"accepted": True}


if __name__ == "__main__":
    if "--iskypro-stdio" not in sys.argv:
        print(
            "This plugin is meant to be run by ISkyPro with --iskypro-stdio.",
            file=sys.stderr,
        )
        sys.exit(2)

    StdioJsonRpcPlugin(PLUGIN_ID, sdk_name="iskypro-python-sdk-v2").run(on_event)
