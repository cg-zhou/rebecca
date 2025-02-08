# 项目启动指南

## 环境要求

- Node.js >= 18
- Python >= 3.11
- Poetry >= 1.7.1

## 环境安装

### 1. 安装 Node.js
- 访问 https://nodejs.org/
- 下载并安装 Node.js 18 或更高版本
- 验证安装：`node --version` 和 `npm --version`

### 2. 安装 Python
- 访问 https://www.python.org/downloads/
- 下载并安装 Python 3.11 或更高版本
- 安装时勾选 "Add Python to PATH"
- 验证安装：`python --version`

### 3. 安装 Poetry
Windows PowerShell：
```powershell
(Invoke-WebRequest -Uri https://install.python-poetry.org -UseBasicParsing).Content | py -
```

其他系统：
```bash
curl -sSL https://install.python-poetry.org | python3 -
```

验证安装：`poetry --version`

### 4. 安装 Make
Windows：
- 安装 Git Bash (推荐，包含make): https://git-scm.com/download/win
- 或安装 MinGW: http://mingw-w64.org/doku.php

Linux：
```bash
sudo apt-get install make
```

macOS：
```bash
xcode-select --install
```

验证安装：`make --version`

## 项目设置

1. 克隆项目：
```bash
git clone [项目地址]
cd deepseek-enchant
```

2. 安装依赖：

前端依赖：
```bash
cd frontend
npm install
```

后端依赖：
```bash
cd backend
poetry install
```

## 启动项目

### 启动前端
```bash
cd frontend
npm run dev
```

### 启动后端
```bash
cd backend
poetry run python main.py
```

## 访问项目

- 前端页面：http://localhost:3000
- 后端API：http://localhost:5000

## 开发建议

1. 建议使用 VSCode 作为开发工具
2. 安装推荐的 VSCode 插件：
   - Python
   - Pylance
   - React DevTools
   - ESLint
   - Prettier

## 常见问题

1. 如果遇到 Poetry 安装问题，可以尝试：
   - 确保 Python 版本正确
   - 使用管理员权限运行安装命令
   
2. 如果依赖安装失败：
   - 清除 Poetry 缓存：`poetry cache clear . --all`
   - 删除 `poetry.lock` 文件后重新安装

3. 前端启动问题：
   - 确保 Node.js 版本正确
   - 删除 node_modules 目录后重新安装
