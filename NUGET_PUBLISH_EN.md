# CefGlue NuGet Package Publishing Guide

## Overview

CefGlue is a .NET binding library for the Chromium Embedded Framework (CEF). This document describes how to build and publish CefGlue projects as NuGet packages.

## Included NuGet Packages

| Package Name | Description | Target Framework |
|--------------|-------------|------------------|
| `CefGlue.Common` | Core library with CEF bindings and cross-platform support | net8.0 |
| `CefGlue.WPF` | WPF platform control support | net8.0-windows |
| `CefGlue.Avalonia` | Avalonia cross-platform UI framework support | net8.0 |

Each package has two architecture versions:
- **x64**: Standard version (no suffix)
- **ARM64**: ARM64 version (package name suffix `.ARM64`)

## Build Requirements

- **.NET SDK**: 8.0 or higher
- **Operating System**: Windows (for building all platform targets)
- **Visual Studio**: 2022 or higher (optional, for Visual C++ toolchain)

## Version Information

Version numbers are defined in `Directory.Build.props`:

```xml
<Version>134.6998.178</Version>
<CefRedistVersion>134.3.9</CefRedistVersion>
```

Version format: `{CEF_MAJOR}.{CEF_BUILD}.{PATCH}`

## Build Steps

### 1. Build All Packages (x64 Architecture)

```powershell
# Build core package
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=x64

# Build WPF package
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=x64

# Build Avalonia package
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=x64
```

### 2. Build ARM64 Architecture Packages

```powershell
# Build core package
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=ARM64

# Build WPF package
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=ARM64

# Build Avalonia package
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=ARM64
```

### 3. Build All Packages at Once

```powershell
# Build all x64 packages
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=x64
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=x64
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=x64

# Build all ARM64 packages
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=ARM64
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=ARM64
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=ARM64
```

## Package Output Location

After building, NuGet packages are output to the following directory:

```
CefGlue/
└── Nuget/
    └── output/
        ├── CefGlue.Common.134.6998.178.nupkg
        ├── CefGlue.Common.ARM64.134.6998.178.nupkg
        ├── CefGlue.WPF.134.6998.178.nupkg
        ├── CefGlue.WPF.ARM64.134.6998.178.nupkg
        ├── CefGlue.Avalonia.134.6998.178.nupkg
        └── CefGlue.Avalonia.ARM64.134.6998.178.nupkg
```

The output path is controlled by the `PackageOutputPath` property in `Directory.Build.props`.

## Package Contents

### CefGlue.Common

The core package contains:
- `Xilium.CefGlue.dll` - CEF native bindings
- `Xilium.CefGlue.Common.dll` - Common functionality
- `Xilium.CefGlue.Common.Shared.dll` - Shared code
- `Xilium.CefGlue.BrowserProcess.exe` - Browser subprocess executable (Windows/Linux/macOS)

### CefGlue.WPF

The WPF package contains:
- `Xilium.CefGlue.WPF.dll` - WPF control implementation
- Depends on `CefGlue.Common` package

### CefGlue.Avalonia

The Avalonia package contains:
- `Xilium.CefGlue.Avalonia.dll` - Avalonia control implementation
- Depends on `CefGlue.Common` package

## Publishing to NuGet.org

### 1. Get API Key

Obtain an API Key from [NuGet.org](https://www.nuget.org/).

### 2. Push Packages

```powershell
# Set API Key (first time use)
dotnet nuget push CefGlue.Common.134.6998.178.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Push all packages
Get-ChildItem Nuget\output\*.nupkg | ForEach-Object {
    dotnet nuget push $_.FullName --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
}
```

### 3. Using NuGet CLI

```powershell
# Set API Key
nuget setapikey YOUR_API_KEY -source https://api.nuget.org/v3/index.json

# Push package
nuget push Nuget\output\CefGlue.Common.134.6998.178.nupkg -source https://api.nuget.org/v3/index.json
```

## Local Package Management

### Copy to Local NuGet Source

```powershell
# Create local NuGet directory (if not exists)
New-Item -ItemType Directory -Force -Path "e:\Work\Godot\260309cef\Nug"

# Copy all packages
Copy-Item "Nuget\output\*.nupkg" -Destination "e:\Work\Godot\260309cef\Nug\" -Force
```

### Configure Local NuGet Source

```powershell
# Add local source
dotnet nuget add source "e:\Work\Godot\260309cef\Nug" --name "LocalCefGlue"

# Or configure in nuget.config
# <?xml version="1.0" encoding="utf-8"?>
# <configuration>
#   <packageSources>
#     <add key="LocalCefGlue" value="e:\Work\Godot\260309cef\Nug" />
#   </packageSources>
# </configuration>
```

## Project Configuration Details

### Directory.Build.props

Global build configuration file that defines:
- Target framework version (`net8.0`)
- Package version number
- Author, description and other metadata
- Output path
- Architecture-related configuration

### Architecture Configuration

```xml
<PropertyGroup Condition="'$(Platform)' == '' or '$(Platform)' == 'x64'">
    <ArchitectureConfig>x64</ArchitectureConfig>
    <PackageSuffix />
</PropertyGroup>

<PropertyGroup Condition="'$(Platform)' == 'ARM64'">
    <ArchitectureConfig>arm64</ArchitectureConfig>
    <PackageSuffix>.ARM64</PackageSuffix>
</PropertyGroup>
```

## Frequently Asked Questions

### Q: Warning NU5104 during build

**Cause**: Stable version package contains pre-release dependencies.

**Solution**: This is a known issue, can be ignored or update dependency versions.

### Q: BrowserProcess build takes a long time

**Cause**: BrowserProcess needs to be published separately for Windows, Linux, and macOS platforms.

**Note**: This is normal behavior, first build may take several minutes.

### Q: How to update CEF version

1. Update version number in `Directory.Build.props`
2. Update `CefRedistVersion`, `CefRedistOSXVersion`, `CefRedistLinuxVersion`
3. Rebuild all packages

## Related Links

- [CEF Official Website](https://bitbucket.org/chromiumembedded/cef/)
- [CefGlue GitHub](https://github.com/OutSystems/CefGlue)
- [NuGet Package Management](https://www.nuget.org/)
