export PYTHONIOENCODING=utf-8
export PYTHONUTF8=1
export PATH := $(PATH);$(USERPROFILE)\AppData\Roaming\Python\Scripts

.PHONY: install clean dev build dev-backend dev-frontend

# Installation
install:
	cd backend && poetry lock && poetry install
	cd frontend && npm install

# Development
dev:
	cmd /c "start /B cmd /c make dev-backend"
	make dev-frontend

dev-backend:
	cd backend && poetry run python -X utf8 main.py

dev-frontend:
	cd frontend && npm run dev

# Build
build:
	cd frontend && npm run build

# Clean
clean:
	rm -rf frontend/dist
	rm -rf frontend/node_modules
	cd backend && poetry env remove --all
