@echo off
setlocal enabledelayedexpansion

rem Publish the WPF app as a single self-contained executable.
dotnet publish CellManager\CellManager\CellManager.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  /p:PublishSingleFile=true ^
  /p:IncludeAllContentForSelfExtract=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true ^
  /p:PublishReadyToRun=true
if errorlevel 1 goto :error

set OUTPUT=CellManager\dist\CellManager.exe
if not exist "%OUTPUT%" goto :missing

echo.
echo ======================================================
echo Self-contained executable ready to distribute:
echo     %OUTPUT%
echo ------------------------------------------------------
for %%F in ("%OUTPUT%") do (
  set SIZE=%%~zF
)
echo File size: %SIZE% bytes
if %SIZE% LSS 100000000 (
  echo WARNING: The executable is smaller than expected.
  echo Make sure the publish completed successfully and
  echo that you are using the file in the dist folder.
)
echo ======================================================
echo.
exit /b 0

:missing
echo Failed to find the self-contained executable at %OUTPUT%.
echo Check the publish output for errors.
exit /b 1

:error
echo dotnet publish failed.
exit /b 1
