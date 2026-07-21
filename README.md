# CefGlue

.NET binding for The Chromium Embedded Framework (CEF).

CefGlue lets you embed Chromium in .NET apps. It is a .NET wrapper control around the Chromium Embedded Framework ([CEF](https://bitbucket.org/chromiumembedded/cef/src/master/)).
It can be used from C# or any other CLR language and provides both Avalonia and WPF web browser control implementations.

## Supported Platforms

| OS      | x64 | ARM64 | WPF | Avalonia |
|---------|-----|-------|-----|----------|
| Windows | ✔️  | ✔️    | ✔️  | ✔️      |
| macOS   | ✔️  | ✔️    | ❌  | ✔️      |
| Linux   | ✔️  | 🔘    | ❌  | ✔️      |

✔️ Supported &nbsp; ❌ Not supported &nbsp; 🔘 Works with issues

See [LINUX.md](./LINUX.md) for more information about issues and tested distribution list.

## NuGet Package Structure

### Core Packages

| Package | Description |
|---------|-------------|
| `CefGlue.Common` | Core managed DLLs. No BrowserProcess bundled. |
| `CefGlue.Common.ARM64` | Core managed DLLs for ARM64 (same DLLs, different CEF native dependency). |
| `CefGlue.Avalonia` | Avalonia web browser control. |
| `CefGlue.WPF` | WPF web browser control (Windows only). |

### BrowserProcess Runtime Packages

The BrowserProcess (CEF renderer subprocess) is no longer bundled inside `CefGlue.Common`. Instead, it is distributed as **standalone runtime packages** — choose **AOT** (NativeAOT, single .exe, no .NET runtime required) or **JIT** (self-contained, includes .NET runtime).

| Package | Mode | Size | Description |
|---------|------|------|-------------|
| `CefGlue.BrowserProcess.runtime.win-x64` | AOT | ~7 MB | NativeAOT, single .exe |
| `CefGlue.BrowserProcess.runtime.win-x64.jit` | JIT | ~36 MB | Self-contained, with coreclr.dll |
| `CefGlue.BrowserProcess.runtime.win-arm64` | AOT | ~7 MB | NativeAOT for ARM64 |
| `CefGlue.BrowserProcess.runtime.win-arm64.jit` | JIT | ~34 MB | Self-contained for ARM64 |
| `CefGlue.BrowserProcess.runtime.linux-x64` | AOT | ~6 MB | NativeAOT for Linux |
| `CefGlue.BrowserProcess.runtime.linux-x64.jit` | JIT | ~36 MB | Self-contained for Linux |
| `CefGlue.BrowserProcess.runtime.linux-arm64` | AOT | ~6 MB | NativeAOT for Linux ARM64 |
| `CefGlue.BrowserProcess.runtime.linux-arm64.jit` | JIT | ~34 MB | Self-contained for Linux ARM64 |
| `CefGlue.BrowserProcess.runtime.osx-x64` | AOT | ~6 MB | NativeAOT for macOS |
| `CefGlue.BrowserProcess.runtime.osx-x64.jit` | JIT | ~35 MB | Self-contained for macOS |
| `CefGlue.BrowserProcess.runtime.osx-arm64` | AOT | ~6 MB | NativeAOT for macOS ARM64 |
| `CefGlue.BrowserProcess.runtime.osx-arm64.jit` | JIT | ~33 MB | Self-contained for macOS ARM64 |

### Meta Packages (convenience)

| Package | Description |
|---------|-------------|
| `CefGlue.BrowserProcess.runtime` | Meta package - includes all AOT platform packages |
| `CefGlue.BrowserProcess.runtime.jit` | Meta package - includes all JIT platform packages |

### AOT vs JIT

| Aspect | AOT (NativeAOT) | JIT (Self-contained) |
|--------|-----------------|----------------------|
| File size | ~6-7 MB single .exe | ~33-36 MB (coreclr + 190+ DLLs) |
| .NET runtime dependency | None | Self-contained, no system dependency |
| Build requirement | AOT workload installed | Standard .NET SDK only |
| Startup time | Faster | Normal |

## Getting Started

### 1. Install CefGlue managed packages

```xml
<ItemGroup>
  <!-- Choose one: -->
  <PackageReference Include="CefGlue.Avalonia" Version="149.7827.156" />
  <!-- or: <PackageReference Include="CefGlue.WPF" Version="149.7827.156" /> -->

  <!-- CefGlue.Common is included transitively, but you can add it explicitly: -->
  <PackageReference Include="CefGlue.Common" Version="149.7827.156" />
</ItemGroup>
```

### 2. Install CEF native binaries

CEF native binaries are distributed through separate NuGet packages. The source depends on your platform:

| Platform | Package | Source |
|----------|---------|--------|
| Windows | `chromiumembeddedframework.runtime.win-x64` | [nuget.org](https://www.nuget.org) |
| Windows ARM64 | `chromiumembeddedframework.runtime.win-arm64` | [nuget.org](https://www.nuget.org) |
| Linux x64 | `cef.redist.linux64` | [GitHub Releases](https://github.com/youfch/cef.redist.linux/releases) |
| Linux ARM64 | `cef.redist.linuxarm64` | [GitHub Releases](https://github.com/youfch/cef.redist.linux/releases) |
| macOS x64 | `cef.redist.osx64` | [GitHub Releases](https://github.com/youfch/cef.redist.osx/releases) |
| macOS ARM64 | `cef.redist.osxarm64` | [GitHub Releases](https://github.com/youfch/cef.redist.osx/releases) |

See [CEF_SETUP.md](./CEF_SETUP.md) for details on setting up CEF native binaries for each platform.

**Windows (from nuget.org):**

```xml
<ItemGroup>
  <PackageReference Include="chromiumembeddedframework.runtime.win-x64" Version="149.0.6" />
  <!-- or the meta package that includes all platforms: -->
  <PackageReference Include="chromiumembeddedframework.runtime" Version="149.0.6" />
</ItemGroup>
```

**Linux (from GitHub Releases):**

First, add the GitHub Releases NuGet source to your `NuGet.config`:

```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  <add key="cef-redist-linux" value="https://github.com/youfch/cef.redist.linux/releases/download/v149.0.4/index.json" />
</packageSources>
```

Then add the package reference:

```xml
<ItemGroup>
  <PackageReference Include="cef.redist.linux64" Version="149.0.4" />
</ItemGroup>
```

**macOS (from GitHub Releases):**

Add the GitHub Releases NuGet source:

```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  <add key="cef-redist-osx" value="https://github.com/youfch/cef.redist.osx/releases/download/v149.0.4/index.json" />
</packageSources>
```

Then add the package reference:

```xml
<ItemGroup>
  <PackageReference Include="cef.redist.osx64" Version="149.0.4" />
</ItemGroup>
```

### 3. Install BrowserProcess runtime package

Choose one of the following patterns:

**A) Cross-platform with RuntimeIdentifier conditions (recommended)**

```xml
<ItemGroup>
  <PackageReference Include="CefGlue.Common" Version="149.7827.156" />

  <!-- Auto-selects the correct platform based on RuntimeIdentifier -->
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'win-x64' Or '$(RuntimeIdentifier)' == ''"
                    Include="CefGlue.BrowserProcess.runtime.win-x64" Version="149.7827.156" />
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'linux-x64'"
                    Include="CefGlue.BrowserProcess.runtime.linux-x64" Version="149.7827.156" />
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'osx-x64'"
                    Include="CefGlue.BrowserProcess.runtime.osx-x64" Version="149.7827.156" />
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'win-arm64'"
                    Include="CefGlue.BrowserProcess.runtime.win-arm64" Version="149.7827.156" />
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'linux-arm64'"
                    Include="CefGlue.BrowserProcess.runtime.linux-arm64" Version="149.7827.156" />
  <PackageReference Condition="'$(RuntimeIdentifier)' == 'osx-arm64'"
                    Include="CefGlue.BrowserProcess.runtime.osx-arm64" Version="149.7827.156" />
</ItemGroup>
```

**B) Single platform (simplest)**

```xml
<PackageReference Include="CefGlue.BrowserProcess.runtime.win-x64" Version="149.7827.156" />
```

**C) Development (JIT, no AOT workload needed)**

```xml
<PackageReference Include="CefGlue.BrowserProcess.runtime.win-x64.jit" Version="149.7827.156" />
```

### 4. Build and run

```bash
dotnet build
dotnet run
```

The BrowserProcess is automatically copied to `$(OutputPath)/CefGlueBrowserProcess/` during build and publish.

### 5. (Optional) AOT publish your own app

```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

## How It Works

```
CefGlue.Common (managed DLLs only, ~330 KB)
  └─ build/CefGlue.Common.targets (CEF resource copy logic)

CefGlue.BrowserProcess.runtime.win-x64 (AOT, ~7 MB)
  └─ build/CefGlue.BrowserProcess.runtime.win-x64.targets
  └─ build/bin/win-x64/Xilium.CefGlue.BrowserProcess.exe

Build output:
  bin/Debug/net10.0/
    ├── locales/                     ← CEF locale files
    ├── CefGlueBrowserProcess/
    │   └── Xilium.CefGlue.BrowserProcess.exe  ← Auto-copied
    ├── libcef.dll (or .so/.dylib)   ← CEF native library
    ├── Xilium.CefGlue.Common.dll
    └── ...
```

## BrowserProcess NativeAOT Build

To build the BrowserProcess with NativeAOT:

```bash
dotnet publish src/CefGlue.BrowserProcess \
  -r win-x64 -c Release \
  -p:PublishAot=true \
  -p:StripSymbols=true \
  -o output/win-x64
```

## Documentation

See the [Avalonia sample](CefGlue.Demo.Avalonia) or [WPF sample](CefGlue.Demo.WPF) projects for example web browsers built with CefGlue.