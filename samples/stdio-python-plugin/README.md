# Python stdio Plugin Sample

Create an installable plugin ZIP with only the Python standard library:

```powershell
python package.py
```

Output:

```text
artifacts/top.iskypro.sample.python-stdio.zip
```

The package script vendors `iskypro_sdk_v2` beside `plugin.py`, so the installed
plugin does not depend on the source repository layout. The target machine must
provide a compatible `python` command. The packager uses the SDK from the public
repository when present, otherwise an installed `iskypro-sdk-v2` package. Set
`ISKYPRO_PYTHON_SDK_PATH` to select a specific SDK package directory.
