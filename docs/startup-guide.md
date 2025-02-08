# 项目启动指南

## 环境要求

- Node.js >= 18
- Python >= 3.11
- Poetry >= 1.7.1
- Make (Git Bash 或 MinGW)

## 环境安装

1. 安装 Node.js: https://nodejs.org/
2. 安装 Python: https://www.python.org/downloads/ (勾选 "Add Python to PATH")
3. 安装 Poetry:
```powershell
(Invoke-WebRequest -Uri https://install.python-poetry.org -UseBasicParsing).Content | py -
```
4. 安装 Make: 通过安装 Git Bash (推荐): https://git-scm.com/download/win

## 项目启动

### 方式一：使用 Make（推荐）

```bash
# 安装所有依赖
make install

# 启动开发环境（同时启动前后端）
make dev
```

### 方式二：手动启动

前端：
```bash
cd frontend
npm install
npm run dev
```

后端：
```bash
cd backend
poetry install
poetry run python -X utf8 main.py
```

## 访问地址

- 前端页面：http://localhost:5173
- 后端API：http://localhost:8000

## 常见问题

1. Poetry 安装失败：使用管理员权限运行安装命令
2. 依赖安装失败：
   - `poetry cache clear . --all`
   - 删除 `poetry.lock` 重新安装
3. 前端启动失败：删除 node_modules 重新安装
