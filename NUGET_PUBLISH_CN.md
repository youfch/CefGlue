# CefGlue NuGet 包发布指南

## 概述

CefGlue 是 Chromium Embedded Framework (CEF) 的 .NET 绑定库。本文档描述如何将 CefGlue 项目构建并发布为 NuGet 包。

## 包含的 NuGet 包

| 包名称 | 描述 | 目标框架 |
|--------|------|----------|
| `CefGlue.Common` | 核心库，包含 CEF 绑定和跨平台支持 | net8.0 |
| `CefGlue.WPF` | WPF 平台控件支持 | net8.0-windows |
| `CefGlue.Avalonia` | Avalonia 跨平台 UI 框架支持 | net8.0 |

每个包都有两个架构版本：
- **x64**: 标准版本 (无后缀)
- **ARM64**: ARM64 版本 (包名后缀 `.ARM64`)

## 构建要求

- **.NET SDK**: 8.0 或更高版本
- **操作系统**: Windows (用于构建所有平台目标)
- **Visual Studio**: 2022 或更高版本 (可选，用于 Visual C++ 工具链)

## 版本信息

版本号在 `Directory.Build.props` 中定义：

```xml
<Version>134.6998.178</Version>
<CefRedistVersion>134.3.9</CefRedistVersion>
```

版本号格式: `{CEF_MAJOR}.{CEF_BUILD}.{PATCH}`

## 构建步骤

### 1. 构建所有包 (x64 架构)

```powershell
# 构建核心包
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=x64

# 构建 WPF 包
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=x64

# 构建 Avalonia 包
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=x64
```

### 2. 构建 ARM64 架构包

```powershell
# 构建核心包
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=ARM64

# 构建 WPF 包
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=ARM64

# 构建 Avalonia 包
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=ARM64
```

### 3. 一键构建所有包

```powershell
# 构建所有 x64 包
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=x64
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=x64
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=x64

# 构建所有 ARM64 包
dotnet build CefGlue.Common\CefGlue.Common.csproj -c Release /p:Platform=ARM64
dotnet build CefGlue.WPF\CefGlue.WPF.csproj -c Release /p:Platform=ARM64
dotnet build CefGlue.Avalonia\CefGlue.Avalonia.csproj -c Release /p:Platform=ARM64
```

## 包输出位置

构建完成后，NuGet 包输出到以下目录：

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

输出路径由 `Directory.Build.props` 中的 `PackageOutputPath` 属性控制。

## 包内容说明

### CefGlue.Common

核心包包含：
- `Xilium.CefGlue.dll` - CEF 原生绑定
- `Xilium.CefGlue.Common.dll` - 公共功能
- `Xilium.CefGlue.Common.Shared.dll` - 共享代码
- `Xilium.CefGlue.BrowserProcess.exe` - 浏览器子进程可执行文件 (Windows/Linux/macOS)

### CefGlue.WPF

WPF 包包含：
- `Xilium.CefGlue.WPF.dll` - WPF 控件实现
- 依赖 `CefGlue.Common` 包

### CefGlue.Avalonia

Avalonia 包包含：
- `Xilium.CefGlue.Avalonia.dll` - Avalonia 控件实现
- 依赖 `CefGlue.Common` 包

## 发布到 NuGet.org

### 1. 获取 API Key

从 [NuGet.org](https://www.nuget.org/) 获取 API Key。

### 2. 推送包

```powershell
# 设置 API Key (首次使用)
dotnet nuget push CefGlue.Common.134.6998.178.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# 推送所有包
Get-ChildItem Nuget\output\*.nupkg | ForEach-Object {
    dotnet nuget push $_.FullName --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
}
```

### 3. 使用 NuGet CLI

```powershell
# 设置 API Key
nuget setapikey YOUR_API_KEY -source https://api.nuget.org/v3/index.json

# 推送包
nuget push Nuget\output\CefGlue.Common.134.6998.178.nupkg -source https://api.nuget.org/v3/index.json
```

## 本地包管理

### 复制到本地 NuGet 源

```powershell
# 创建本地 NuGet 目录 (如果不存在)
New-Item -ItemType Directory -Force -Path "e:\Work\Godot\260309cef\Nug"

# 复制所有包
Copy-Item "Nuget\output\*.nupkg" -Destination "e:\Work\Godot\260309cef\Nug\" -Force
```

### 配置本地 NuGet 源

```powershell
# 添加本地源
dotnet nuget add source "e:\Work\Godot\260309cef\Nug" --name "LocalCefGlue"

# 或者在 nuget.config 中配置
# <?xml version="1.0" encoding="utf-8"?>
# <configuration>
#   <packageSources>
#     <add key="LocalCefGlue" value="e:\Work\Godot\260309cef\Nug" />
#   </packageSources>
# </configuration>
```

## 项目配置详解

### Directory.Build.props

全局构建配置文件，定义了：
- 目标框架版本 (`net8.0`)
- 包版本号
- 作者、描述等元数据
- 输出路径
- 架构相关配置

### 架构配置

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

## 常见问题

### Q: 构建时出现警告 NU5104

**原因**: 包的稳定版本包含预发布依赖项。

**解决**: 这是已知问题，可以忽略或更新依赖项版本。

### Q: BrowserProcess 构建时间较长

**原因**: BrowserProcess 需要为 Windows、Linux 和 macOS 三个平台分别发布。

**说明**: 这是正常行为，首次构建可能需要几分钟。

### Q: 如何更新 CEF 版本

1. 更新 `Directory.Build.props` 中的版本号
2. 更新 `CefRedistVersion`、`CefRedistOSXVersion`、`CefRedistLinuxVersion`
3. 重新构建所有包

## 相关链接

- [CEF 官方网站](https://bitbucket.org/chromiumembedded/cef/)
- [CefGlue GitHub](https://github.com/OutSystems/CefGlue)
- [NuGet 包管理](https://www.nuget.org/)
