# Rebecca 媒体库（开发中...） 🎬

Rebecca是一个本地媒体管理工具，用于自动获取电影信息和海报图。

## 功能特点 ✨

- **一键扫描** 📂: 自动扫描视频文件夹
- **智能识别** 🔍: 接入TMDB获取电影信息和海报
- **自动生成NFO** 📝: 生成元数据文件
- **实时反馈** ⚡: 通过WebSocket显示处理进度
- **后台运行** 🏃‍♂️: 可最小化到系统托盘

## 技术小笔记 🛠️

应用使用的技术栈：

- 后端: **.NET 8**和**ASP.NET Core**
- 桌面: **WPF**和**WebView2**
- 前端: **Vue 3**和**Element Plus**
- 通信: **RESTful API**和**WebSockets**

## 媒体库核心功能 🌟

1. **配置** ⚙️
   - 添加媒体文件夹
   - 设置TMDB的API密钥

2. **扫描** 🔎
   - 支持多种视频格式(.mp4, .mkv等)
   - 可查看进度和取消操作

3. **处理** 🧠
   - 从文件名分析电影标题
   - 下载海报和背景图
   - 生成NFO信息文件

4. **界面** 😊
   - 展示媒体文件和处理状态
   - 一键扫描功能

## 开发环境 💻

- Visual Studio 2022
- Node.js 16+
- .NET 8 SDK

## 项目结构 📁

- **Rebecca**: 主应用和API服务
- **StdEx**: 工具库
- **StdEx.Tests**: 单元测试
- **web**: 前端应用