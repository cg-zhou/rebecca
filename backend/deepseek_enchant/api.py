from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class Message(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    messages: List[Message]

@app.post("/chat")
async def chat(request: ChatRequest):
    # 获取最后一条用户消息
    last_message = request.messages[-1].content
    return {
        "response": f"你发送的消息是：{last_message}"
    }

@app.get("/")
async def root():
    return {"message": "Hello from DeepSeek Enchant"}
