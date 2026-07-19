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
| `CefGlue.Avalonia` | Avalonia web browser control. |
| `CefGlue.WPF` | WPF web browser control. |

### BrowserProcess Runtime Packages

The BrowserProcess (CEF renderer subprocess) is no longer bundled inside `CefGlue.Common`. Instead, it is distributed as **standalone runtime packages** — choose **AOT** (NativeAOT, single .exe, no .NET runtime required) or **JIT** (self-contained, includes .NET runtime).

| Package | Mode | Size | Description |
|---------|------|------|-------------|
| `CefGlue.BrowserProcess.runtime.win-x64` | AOT | ~7 MB | NativeAOT, single .exe |
| `CefGlue.BrowserProcess.runtime.win-x64.jit` | JIT | ~36 MB | Self-contained, with coreclr.dll |
| `CefGlue.BrowserProcess.runtime.win-arm64` | AOT | ~7 MB | NativeAOT for ARM64 |
| `CefGlue.BrowserProcess.runtime.win-arm64.jit` | JIT | ~35 MB | Self-contained for ARM64 |
| `CefGlue.BrowserProcess.runtime.linux-x64` | AOT | ~50 MB | NativeAOT for Linux |
| `CefGlue.BrowserProcess.runtime.linux-x64.jit` | JIT | ~36 MB | Self-contained for Linux |
| `CefGlue.BrowserProcess.runtime.linux-arm64` | AOT | ~50 MB | NativeAOT for Linux ARM64 |
| `CefGlue.BrowserProcess.runtime.linux-arm64.jit` | JIT | ~34 MB | Self-contained for Linux ARM64 |
| `CefGlue.BrowserProcess.runtime.osx-x64` | AOT | ~50 MB | NativeAOT for macOS |
| `CefGlue.BrowserProcess.runtime.osx-x64.jit` | JIT | ~35 MB | Self-contained for macOS |
| `CefGlue.BrowserProcess.runtime.osx-arm64` | AOT | ~50 MB | NativeAOT for macOS ARM64 |
| `CefGlue.BrowserProcess.runtime.osx-arm64.jit` | JIT | ~33 MB | Self-contained for macOS ARM64 |

### Meta Packages (convenience)

| Package | Description |
|---------|-------------|
| `CefGlue.BrowserProcess.runtime` | Meta package - includes all AOT platform packages |
| `CefGlue.BrowserProcess.runtime.jit` | Meta package - includes all JIT platform packages |
| `CefGlue.BrowserProcess.runtime.win` | Meta package - includes Windows AOT (x64 + arm64) |
| `CefGlue.BrowserProcess.runtime.win.jit` | Meta package - includes Windows JIT (x64 + arm64) |

### AOT vs JIT

| Aspect | AOT (NativeAOT) | JIT (Self-contained) |
|--------|-----------------|----------------------|
| File size | ~7-50 MB single .exe | ~33-70 MB (coreclr + 190+ DLLs) |
| .NET runtime dependency | None | Self-contained, no system dependency |
| Build requirement | AOT workload installed | Standard .NET SDK only |
| Startup time | Faster | Normal |
| Recommendation | **Production release** | Development / CI without AOT |

## Getting Started

### 1. Install CefGlue and CEF native binaries

```xml
<ItemGroup>
  <!-- CefGlue managed packages -->
  <PackageReference Include="CefGlue.Avalonia" Version="149.7827.156" />
  <!-- or: <PackageReference Include="CefGlue.WPF" Version="149.7827.156" /> -->

  <!-- CEF native binaries (required) -->
  <PackageReference Include="chromiumembeddedframework.runtime.win-x64" Version="149.0.6" />
  <!-- or: <PackageReference Include="chromiumembeddedframework.runtime" Version="149.0.6" /> -->
</ItemGroup>
```

### 2. Install BrowserProcess runtime package

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

### 3. Build and run

```bash
dotnet build
dotnet run
```

The BrowserProcess is automatically copied to `$(OutputPath)/CefGlueBrowserProcess/` during build and publish.

### 4. (Optional) AOT publish your own app

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
    ├── CefGlueBrowserProcess/
    │   └── Xilium.CefGlue.BrowserProcess.exe  ← Auto-copied
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