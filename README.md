# CellManager Build Instructions

## Prerequisites
- [Microsoft .NET SDK 8.0 or later](https://dotnet.microsoft.com/en-us/download)
- Windows build host (publishing targets `win-x64`).

## Restore Dependencies
```bash
dotnet restore CellManager/CellManager.sln
```

## Build (Debug)
Use the default framework-dependent build for local debugging:
```bash
dotnet build CellManager/CellManager.sln
```

## Publish Single-File Release
The Release configuration is pre-configured to produce a **single executable** that embeds every managed and native dependency *and* the required .NET runtime for `win-x64`. The published `CellManager.exe` can therefore run on target machines that do **not** have any version of the .NET Framework or .NET Runtime installed.

### Command-line publish
Run the following from the repository root:
```bash
dotnet publish CellManager/CellManager/CellManager.csproj -c Release
```
The packaged executable will be generated at:
```
CellManager/CellManager/bin/Release/net8.0-windows/win-x64/publish/CellManager.exe
```
Only that executable (and any content files you intentionally copy next to it) needs to be distributed.

### Visual Studio publish profile
When using Visual Studio, select the **SelfContainedWinX64** publish profile. It writes the finished package to:
```
CellManager/CellManager/bin/Publish/SelfContainedWinX64/CellManager.exe
```
This profile enforces the same single-file, self-contained settings as the command-line publish, so the output runs without installing any .NET runtime.

> 💡 Only the published output is self-contained. The build output in `bin/Release/net8.0-windows` (or any Debug build) still expects the .NET runtime to be present and is meant for local development.

If you need to publish for a different runtime, override the runtime identifier (e.g., `-r win-x86`) and adjust self-contained options as necessary.
