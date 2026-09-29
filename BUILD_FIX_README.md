# Invenzo Build Fix

The original Visual Studio error was `CS0016: There is not enough space on the disk` while writing the generated DLL. This package contains no `bin/` or `obj/` folders. If the D: drive exists, `Directory.Build.props` routes build output/intermediate files to `D:\InvenzoBuild` so C: does not fill during compilation. If D: is unavailable, the project keeps the normal output location.

## Build
1. Extract the ZIP.
2. Open the `v19work` folder/solution in Visual Studio.
3. Right-click the project -> **Open in Terminal**.
4. Run `powershell -ExecutionPolicy Bypass -File .\BUILD_FIX.ps1` or run `dotnet restore` then `dotnet build`.

The NU1900 vulnerability-data warning was disabled because it was caused by NuGet being unable to reach `api.nuget.org`; it is not a source-code compilation error. Two migration class names were capitalized to remove CS8981 warnings while keeping their original migration IDs unchanged.


## MSB3027 / MSB3021 fix
The project now stops a running `InventoryManagementSystem.exe` before MSBuild writes new output. This prevents the common “file is locked by another process” error when Visual Studio is rebuilt while the previous debug instance is still running.
