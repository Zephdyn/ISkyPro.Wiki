from __future__ import annotations

import sys
from pathlib import Path

plugin_directory = Path(__file__).resolve().parent
vendored_sdk = plugin_directory / "iskypro_sdk_v2"
sdk_root = plugin_directory if vendored_sdk.is_dir() else plugin_directory.parents[1] / "sdk" / "python"
sys.path.insert(0, str(sdk_root))

from iskypro_sdk_v2 import StdioJsonRpcPlugin  # noqa: E402


PLUGIN_ID = "top.iskypro.sample.python-stdio"


def on_event(message, context):
    event_id = message.event_id
    content = message.text
    print(f"python sample received {event_id}", file=sys.stderr)
    context.log_write(f"python sample handled {event_id}")
    if content:
        message.reply(f"python echo: {content}")
    return {"accepted": True}


if __name__ == "__main__":
    if "--iskypro-stdio" not in sys.argv:
        print(
            "This plugin is meant to be run by ISkyPro with --iskypro-stdio.",
            file=sys.stderr,
        )
        sys.exit(2)

    StdioJsonRpcPlugin(PLUGIN_ID, sdk_name="iskypro-python-sdk-v2").run(on_event)
