# CellManager Build Instructions

## Prerequisites
- [Microsoft .NET SDK 8.0 or later](https://dotnet.microsoft.com/en-us/download)
- Windows build host (publishing targets `win-x64`).

## Restore Dependencies
```bash
dotnet restore CellManager/CellManager.sln
```

## Build (Debug)
If you want the fastest local build (framework-dependent), disable the self-contained settings temporarily:
```bash
dotnet build CellManager/CellManager.sln -c Debug /p:SelfContained=false /p:PublishSingleFile=false
```

## Publish Single-File Release
The project is now configured to emit a **single executable** that embeds every managed and native dependency *and* the required .NET runtime for `win-x64`. Whenever you publish without overriding the defaults, the produced `CellManager.exe` can run on machines that do **not** have any version of the .NET Framework or .NET Runtime installed.

### One-command publish (recommended)
Run the helper script from the repository root:
```powershell
./publish-self-contained.bat
```
It executes the correct `dotnet publish` command and then copies the finished single file to `CellManager/dist/CellManager.exe`. **Send only this file** to other people—the file in `dist` is guaranteed to have the runtime baked in.

### Command-line publish
Run the following from the repository root:
```bash
dotnet publish CellManager/CellManager/CellManager.csproj -c Release
```
The packaged executable will be generated at:
```
CellManager/CellManager/bin/Release/net8.0-windows/win-x64/publish/CellManager.exe
```
After publishing, the build copies that executable to:
```
CellManager/dist/CellManager.exe
```
Only that executable (and any content files you intentionally copy next to it) needs to be distributed.

> ❗ If someone still sees a ".NET runtime required" prompt, double-check that you sent them `CellManager/dist/CellManager.exe`. Files from `bin/Debug` or `bin/Release` **outside** the `publish` folder remain framework-dependent and will require an external runtime.

> ❗ **Check the file size** – a self-contained single file will be well over 100 MB. If you see an output that is only a few megabytes, you are still looking at a framework-dependent build. Clean the `bin`/`obj` folders and republish to get the bundled executable.

### Visual Studio publish profile
When using Visual Studio, select the **SelfContainedWinX64** publish profile. It writes the finished package to:
```
CellManager/CellManager/bin/Publish/SelfContainedWinX64/CellManager.exe
```
This profile enforces the same single-file, self-contained settings as the command-line publish, so the output runs without installing any .NET runtime.

> 💡 The default configuration now prefers the self-contained single-file output. If you need a lightweight framework-dependent build (for example, faster Debug iterations), pass `/p:SelfContained=false /p:PublishSingleFile=false` on your `build` command.

If you need to publish for a different runtime, override the runtime identifier (e.g., `-r win-x86`) and adjust self-contained options as necessary.
