# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概况
M3U8/HLS 视频流下载工具，基于 .NET 10 + Avalonia UI，跨平台（Win/Mac/Linux）桌面应用。支持多线程下载、AES 解密、插件系统、REST API 控制。

本项目为 [Harlan-H/M3u8Downloader_H](https://github.com/Harlan-H/M3u8Downloader_H) 的二开分支，作者 XyzBeta，MIT 协议。程序集名称 `FlowGet`。

## 构建与运行

```bash
# 构建整个解决方案
dotnet build FlowGet.sln

# 运行桌面应用
dotnet run --project FlowGet

# 发布单文件可执行文件 (以 win-x64 为例)
dotnet publish FlowGet -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# 发布 macOS (自动生成 .app 包)
dotnet publish FlowGet -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
```

publish 时会自动触发 FFmpeg 下载（`DownloadFFmpeg.ps1`），无需手动准备。macOS 发布还会自动调用 `pack-macos.ps1` 打包 .app 包体。

## 核心架构

分层架构：**Abstractions（接口契约）→ Common（公共工具）→ 领域模块（M3U8/Downloader/Combiners）→ Core（编排）→ RestServer → 主应用**

### 解决方案项目 (8 个)

```
FlowGet (主应用, Avalonia UI, net10.0)
  ├── FlowGet.Core (下载编排层)
  │     ├── FlowGet.Combiners (TS 合并/FFmpeg 转码)
  │     ├── FlowGet.Downloader (TS 分段下载引擎)
  │     └── FlowGet.M3U8 (M3U8 解析库)
  ├── FlowGet.RestServer (嵌入 HTTP API)
  ├── FlowGet.Common (公共模型/扩展/设置基类)
  └── FlowGet.Abstractions (纯接口，无依赖，供插件引用)
```

### 各项目职责

- **`FlowGet.Abstractions`** — 所有接口定义，插件开发的 SDK。`IDownloader`、`IM3uFileReader`、`IDownloadService` 等核心契约
- **`FlowGet.Common`** — 共享实现：`SettingsBase`(JSON 持久化)、`DownloadParams`、AES 解密扩展、路径工具
- **`FlowGet.M3U8`** — M3U8 解析引擎。`M3uFileReaderManager` 拉取内容，`M3UFileReaderWithStream` 按行解析，各 `IAttributeReader` 处理 HLS 标签
- **`FlowGet.Downloader`** — 下载引擎。`M3u8Downloader`(多线程)、`LiveM3uDownloader`(直播轮询)、`CryptM3uDownloader`(AES 解密)、`MediaDownloader`
- **`FlowGet.Combiners`** — 合并/转码。`M3uCombiner`(fMP4 拼接) 和 `FFmpeg`(通过 CliWrap 调用 ffmpeg)
- **`FlowGet.Core`** — 编排层。`DownloaderCoreClient` 暴露 `IDownloader`，协调解析→下载→合并→清理全流程
- **`FlowGet.RestServer`** — 内嵌 HTTP API。`HttpListenService` 监听自动端口（65432→65400），5 个端点供外部调用下载
- **`FlowGet`** — Avalonia 桌面应用。MVVM 模式，`App.axaml.cs` 配置 DI 容器，`ViewLocator` 按命名约定路由 View→ViewModel。入口 `DashboardWindow`

### 关键模式

- **MVVM**: CommunityToolkit.Mvvm 8.4.2，View 与 ViewModel 一一对应，通过 `FrameWork/ViewLocator.cs` 映射
- **DI**: `Microsoft.Extensions.DependencyInjection`，在 `App.axaml.cs` 中注册所有 View/ViewModel/Service 为单例
- **UI 框架**: Avalonia 12.0.3 + Material.Avalonia 3.17 + DialogHost.Avalonia 0.12.2
- **插件**: 实现 `IPluginEntry` 接口，打包为 zip（含 manifest.json + 程序集），放到插件目录即被发现加载。插件系统内嵌在主项目中（非独立项目），`Assets/flowget-extension.zip` 为嵌入式资源
- **HTTP 拦截**: 所有网络请求走 `Utils/Http.cs` 的 HttpClient，支持代理配置和账号密码认证
- **设置持久化**: `Services/SettingsService.cs` 继承 `SettingsBase`，JSON 序列化到本地文件
- **FFmpeg 集成**: publish 时通过 `DownloadFFmpeg.ps1` 自动下载对应平台的 ffmpeg，运行时通过 CliWrap 调用

## 注意事项

- 解决方案中无测试项目，无 CI/CD workflows
- 源码、注释、文档均为中文
- 项目使用 MIT 协议
