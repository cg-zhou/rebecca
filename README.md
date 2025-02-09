# DeepSeek Enchant

基于 DeepSeek 的个性化 AI 智能体探索项目 | Exploring personalized AI agents with DeepSeek

![项目预览](/docs/preview.png)

## 项目简介 | Introduction

本项目旨在探索和开发基于 DeepSeek 的 AI 智能体，通过深度学习技术打造实用的 AI 助手。

This project aims to explore and develop AI agents based on DeepSeek, creating practical AI assistants through deep learning technology.

## 特性 | Features

- 🤖 基于 DeepSeek 大语言模型 | Based on DeepSeek LLM
- 🔄 持续学习与优化 | Continuous Learning and Optimization

## 开发计划 | Roadmap

- [ ] DeepSeek API 接入 | DeepSeek API Integration
- [ ] 智能体基础框架搭建 | Basic Agent Framework Setup
- [ ] 交互界面开发 | UI Development

## 技术栈 | Tech Stack

- DeepSeek API | DeepSeek 接口
- Python | Python 编程语言
- FastAPI | FastAPI 后端框架
- Vue.js | Vue.js 前端框架

## 开始使用 | Getting Started

```bash
# 克隆仓库 | Clone the repository
git clone https://github.com/cg-zhou/deepseek-enchant.git
cd deepseek-enchant

# 安装依赖 | Install dependencies
make install

# 启动开发环境 | Start development
make dev
```

访问地址 | Access URLs:
- 前端 | Frontend: http://localhost:5173 
- 后端 | Backend: http://localhost:8000

### 使用 Make 命令（推荐）| Using Make Commands (Recommended)

```bash
# 克隆仓库 | Clone the repository
git clone https://github.com/cg-zhou/deepseek-enchant.git
cd deepseek-enchant

# 安装所有依赖 | Install all dependencies
make install

# 启动开发环境 | Start development environment
make dev

# 运行测试 | Run tests
make test

# 构建项目 | Build project
make build

# 清理构建文件 | Clean build files
make clean
```

### 手动启动 | Manual Setup

```bash
# 克隆仓库 | Clone the repository
git clone https://github.com/cg-zhou/deepseek-enchant.git
cd deepseek-enchant

# 安装前端依赖 | Install frontend dependencies
cd frontend && npm install

# 安装后端依赖 | Install backend dependencies
cd ../backend && poetry install

# 配置环境变量 | Configure environment variables
cp .env.example .env

# 启动后端服务 | Start backend server
cd backend && poetry run python main.py

# 新开终端，启动前端服务 | Open new terminal, start frontend server
cd frontend && npm run dev
```

## 许可证 | License

MIT 开源许可证 | MIT License
