# CefGlue

Chromium Embedded Framework (CEF) 的 .NET 绑定。

CefGlue 让你在 .NET 应用中嵌入 Chromium 浏览器。它是 [CEF](https://bitbucket.org/chromiumembedded/cef/src/master/) 的 .NET 包装控件，支持 Avalonia 和 WPF 两种 Web 浏览器控件实现。

## 支持平台

| 操作系统 | x64 | ARM64 | WPF | Avalonia |
|---------|-----|-------|-----|----------|
| Windows | ✔️  | ✔️    | ✔️  | ✔️      |
| macOS   | ✔️  | ✔️    | ❌  | ✔️      |
| Linux   | ✔️  | 🔘    | ❌  | ✔️      |

✔️ 支持 &nbsp; ❌ 不支持 &nbsp; 🔘 有已知问题

更多 Linux 相关问题和测试发行版列表请参阅 [LINUX.md](./LINUX.md)。

## NuGet 包结构

### 核心包

| 包名 | 说明 |
|------|------|
| `CefGlue.Common` | 核心托管 DLL，不包含 BrowserProcess。 |
| `CefGlue.Common.ARM64` | ARM64 版核心托管 DLL（DLL 相同，CEF 依赖不同）。 |
| `CefGlue.Avalonia` | Avalonia Web 浏览器控件。 |
| `CefGlue.WPF` | WPF Web 浏览器控件（仅 Windows）。 |

### BrowserProcess 运行时包

BrowserProcess（CEF 渲染子进程）不再内嵌在 `CefGlue.Common` 中，而是作为**独立的运行时包**分发。可选择 **AOT**（NativeAOT 单文件，无需 .NET 运行时）或 **JIT**（自包含，包含 .NET 运行时）。

| 包名 | 模式 | 大小 | 说明 |
|------|------|------|------|
| `CefGlue.BrowserProcess.runtime.win-x64` | AOT | ~7 MB | NativeAOT 单文件 exe |
| `CefGlue.BrowserProcess.runtime.win-x64.jit` | JIT | ~36 MB | 自包含，含 coreclr.dll |
| `CefGlue.BrowserProcess.runtime.win-arm64` | AOT | ~7 MB | ARM64 的 NativeAOT |
| `CefGlue.BrowserProcess.runtime.win-arm64.jit` | JIT | ~34 MB | ARM64 的自包含 |
| `CefGlue.BrowserProcess.runtime.linux-x64` | AOT | ~6 MB | Linux 的 NativeAOT |
| `CefGlue.BrowserProcess.runtime.linux-x64.jit` | JIT | ~36 MB | Linux 的自包含 |
| `CefGlue.BrowserProcess.runtime.linux-arm64` | AOT | ~6 MB | Linux ARM64 的 NativeAOT |
| `CefGlue.BrowserProcess.runtime.linux-arm64.jit` | JIT | ~34 MB | Linux ARM64 的自包含 |
| `CefGlue.BrowserProcess.runtime.osx-x64` | AOT | ~6 MB | macOS 的 NativeAOT |
| `CefGlue.BrowserProcess.runtime.osx-x64.jit` | JIT | ~35 MB | macOS 的自包含 |
| `CefGlue.BrowserProcess.runtime.osx-arm64` | AOT | ~6 MB | macOS ARM64 的 NativeAOT |
| `CefGlue.BrowserProcess.runtime.osx-arm64.jit` | JIT | ~33 MB | macOS ARM64 的自包含 |

### 元包（便捷安装）

| 包名 | 说明 |
|------|------|
| `CefGlue.BrowserProcess.runtime` | 元包 - 包含所有 AOT 平台包 |
| `CefGlue.BrowserProcess.runtime.jit` | 元包 - 包含所有 JIT 平台包 |

### AOT vs JIT 对比

| 对比项 | AOT (NativeAOT) | JIT (自包含) |
|--------|-----------------|-------------|
| 文件大小 | ~6-7 MB 单文件 exe | ~33-36 MB (coreclr + 190+ DLL) |
| .NET 运行时依赖 | 无 | 自包含，不需系统安装运行时 |
| 构建要求 | 需安装 AOT workload | 标准 .NET SDK 即可 |
| 启动速度 | 更快 | 正常 |

## 快速开始

### 1. 安装 CefGlue 托管包

```xml
<ItemGroup>
  <!-- 二选一 -->
  <PackageReference Include="CefGlue.Avalonia" Version="149.7827.156" />
  <!-- 或: <PackageReference Include="CefGlue.WPF" Version="149.7827.156" /> -->

  <!-- CefGlue.Common 会被传递引用，也可显式添加 -->
  <PackageReference Include="CefGlue.Common" Version="149.7827.156" />
</ItemGroup>
```

### 2. 安装 CEF 原生二进制

CEF 原生二进制通过独立的 NuGet 包分发，来源因平台而异：

| 平台 | 包名 | 来源 |
|------|------|------|
| Windows x64 | `chromiumembeddedframework.runtime.win-x64` | [nuget.org](https://www.nuget.org) |
| Windows ARM64 | `chromiumembeddedframework.runtime.win-arm64` | [nuget.org](https://www.nuget.org) |
| Linux x64 | `cef.redist.linux64` | [GitHub Releases](https://github.com/youfch/cef.redist.linux/releases) |
| Linux ARM64 | `cef.redist.linuxarm64` | [GitHub Releases](https://github.com/youfch/cef.redist.linux/releases) |
| macOS x64 | `cef.redist.osx64` | [GitHub Releases](https://github.com/youfch/cef.redist.osx/releases) |
| macOS ARM64 | `cef.redist.osxarm64` | [GitHub Releases](https://github.com/youfch/cef.redist.osx/releases) |

详细说明请参阅 [CEF_SETUP.md](./CEF_SETUP.md)（英文）。

**Windows（从 nuget.org）：**

```xml
<ItemGroup>
  <PackageReference Include="chromiumembeddedframework.runtime.win-x64" Version="149.0.6" />
  <!-- 或元包： -->
  <PackageReference Include="chromiumembeddedframework.runtime" Version="149.0.6" />
</ItemGroup>
```

**Linux（从 GitHub Releases）：**

在 `NuGet.config` 中添加包源：

```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  <add key="cef-redist-linux" value="https://github.com/youfch/cef.redist.linux/releases/download/v149.0.4/index.json" />
</packageSources>
```

然后添加包引用：

```xml
<ItemGroup>
  <PackageReference Include="cef.redist.linux64" Version="149.0.4" />
</ItemGroup>
```

**macOS（从 GitHub Releases）：**

在 `NuGet.config` 中添加包源：

```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  <add key="cef-redist-osx" value="https://github.com/youfch/cef.redist.osx/releases/download/v149.0.4/index.json" />
</packageSources>
```

然后添加包引用：

```xml
<ItemGroup>
  <PackageReference Include="cef.redist.osx64" Version="149.0.4" />
</ItemGroup>
```

### 3. 安装 BrowserProcess 运行时包

选择以下方式之一：

**方式 A：跨平台，按 RuntimeIdentifier 自动选择（推荐）**

```xml
<ItemGroup>
  <PackageReference Include="CefGlue.Common" Version="149.7827.156" />

  <!-- 根据 RuntimeIdentifier 自动选择对应平台 -->
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

**方式 B：单平台（最简单）**

```xml
<PackageReference Include="CefGlue.BrowserProcess.runtime.win-x64" Version="149.7827.156" />
```

**方式 C：开发环境（JIT，无需 AOT workload）**

```xml
<PackageReference Include="CefGlue.BrowserProcess.runtime.win-x64.jit" Version="149.7827.156" />
```

### 4. 构建和运行

```bash
dotnet build
dotnet run
```

BrowserProcess 在构建和发布时会自动复制到 `$(OutputPath)/CefGlueBrowserProcess/` 目录。

### 5. （可选）AOT 发布你的应用

```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

## 工作原理

```
CefGlue.Common (仅托管 DLL，约 330 KB)
  └─ build/CefGlue.Common.targets (CEF 资源复制逻辑)

CefGlue.BrowserProcess.runtime.win-x64 (AOT, ~7 MB)
  └─ build/CefGlue.BrowserProcess.runtime.win-x64.targets
  └─ build/bin/win-x64/Xilium.CefGlue.BrowserProcess.exe

构建输出:
  bin/Debug/net10.0/
    ├── locales/                     ← CEF 本地化文件
    ├── CefGlueBrowserProcess/
    │   └── Xilium.CefGlue.BrowserProcess.exe  ← 自动复制
    ├── libcef.dll (或 .so/.dylib)   ← CEF 原生库
    ├── Xilium.CefGlue.Common.dll
    └── ...
```

## BrowserProcess NativeAOT 构建

使用 NativeAOT 构建 BrowserProcess：

```bash
dotnet publish src/CefGlue.BrowserProcess \
  -r win-x64 -c Release \
  -p:PublishAot=true \
  -p:StripSymbols=true \
  -o output/win-x64
```

## 文档

请参阅 [Avalonia 示例](CefGlue.Demo.Avalonia) 或 [WPF 示例](CefGlue.Demo.WPF) 项目，了解使用 CefGlue 构建的 Web 浏览器示例。