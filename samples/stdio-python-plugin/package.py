from __future__ import annotations

import json
import importlib.util
import os
import shutil
import zipfile
from pathlib import Path


ROOT = Path(__file__).resolve().parent
ARTIFACTS = ROOT / "artifacts"
STAGING = ARTIFACTS / "package"


def resolve_sdk_package() -> Path:
    configured = os.environ.get("ISKYPRO_PYTHON_SDK_PATH")
    if configured:
        return Path(configured).resolve()

    repository_package = (ROOT.parents[1] / "sdk" / "python" / "iskypro_sdk_v2").resolve()
    if repository_package.is_dir():
        return repository_package

    installed = importlib.util.find_spec("iskypro_sdk_v2")
    if installed and installed.submodule_search_locations:
        return Path(next(iter(installed.submodule_search_locations))).resolve()

    raise RuntimeError(
        "Python SDK package not found. Install iskypro-sdk-v2 or set "
        "ISKYPRO_PYTHON_SDK_PATH."
    )


def main() -> None:
    sdk_package = resolve_sdk_package()
    manifest_path = ROOT / "manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    plugin_id = str(manifest.get("pluginId") or "").strip()
    if not plugin_id:
        raise RuntimeError("manifest.json must contain pluginId")
    if not sdk_package.is_dir():
        raise RuntimeError(f"Python SDK package not found: {sdk_package}")

    shutil.rmtree(STAGING, ignore_errors=True)
    STAGING.mkdir(parents=True)
    shutil.copy2(manifest_path, STAGING / "manifest.json")
    shutil.copy2(ROOT / "plugin.py", STAGING / "plugin.py")
    if (ROOT / "README.md").is_file():
        shutil.copy2(ROOT / "README.md", STAGING / "README.md")
    shutil.copytree(
        sdk_package,
        STAGING / "iskypro_sdk_v2",
        ignore=shutil.ignore_patterns("__pycache__", "*.pyc", "*.pyo"),
    )

    ARTIFACTS.mkdir(exist_ok=True)
    archive_path = ARTIFACTS / f"{plugin_id}.zip"
    archive_path.unlink(missing_ok=True)
    with zipfile.ZipFile(archive_path, "w", zipfile.ZIP_DEFLATED) as archive:
        for path in sorted(STAGING.rglob("*")):
            if path.is_file():
                archive.write(path, path.relative_to(STAGING).as_posix())

    shutil.rmtree(STAGING)
    print(f"ISkyPro plugin package: {archive_path}")


if __name__ == "__main__":
    main()
