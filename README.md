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
The Release configuration is pre-configured to produce a self-contained, single-file executable that bundles all dependent DLLs and the required .NET runtime for `win-x64`. The published executable can therefore run on target machines that do **not** have any version of the .NET Framework or .NET Runtime installed.
Run the following from the repository root:
```bash
dotnet publish CellManager/CellManager/CellManager.csproj -c Release
```
The packaged executable will be generated at:
```
CellManager/CellManager/bin/Release/net8.0-windows/win-x64/publish/CellManager.exe
```

> 💡 Only the published output is self-contained. The build output in `bin/Release/net8.0-windows` (or any Debug build) still expects the .NET runtime to be present and is meant for local development.

If you need to publish for a different runtime, override the runtime identifier (e.g., `-r win-x86`) and adjust self-contained options as necessary.
