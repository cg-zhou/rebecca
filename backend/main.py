# -*- coding: utf-8 -*-
import sys
import socket
import psutil
from fastapi import FastAPI
import uvicorn

# 确保使用 UTF-8 编码
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

def kill_process_on_port(port: int) -> None:
    for proc in psutil.process_iter(['pid', 'name', 'connections']):
        try:
            for conn in proc.connections():
                if conn.laddr.port == port:
                    proc.kill()
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            pass

def is_port_in_use(port: int) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        try:
            s.bind(('127.0.0.1', port))
            return False
        except socket.error:
            return True

app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Hello from DeepSeek Enchant"}

def find_available_port(start_port: int = 8000, max_port: int = 8020) -> int:
    for port in range(start_port, max_port):
        if is_port_in_use(port):
            kill_process_on_port(port)
        if not is_port_in_use(port):
            return port
    raise RuntimeError(f"No available ports in range {start_port}-{max_port}")

if __name__ == "__main__":
    try:
        port = find_available_port()
        print(f"Starting server on port {port}")
        uvicorn.run(app, host="127.0.0.1", port=port, log_level="info")
    except KeyboardInterrupt:
        print("\nShutting down server...")
        sys.exit(0)
