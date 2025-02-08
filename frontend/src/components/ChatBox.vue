<template>
  <div class="chat-container">
    <div class="chat-messages" ref="messagesContainer">
      <div v-for="(msg, index) in messages" 
           :key="index" 
           :class="['message', msg.role === 'user' ? 'user-message' : 'bot-message']">
        <div class="message-content">
          <div class="message-header">
            {{ msg.role === 'user' ? '用户' : 'AI' }}
          </div>
          <div class="message-text">{{ msg.content }}</div>
        </div>
      </div>
    </div>
    
    <div class="input-container">
      <textarea
        v-model="currentMessage"
        @keydown.enter.prevent="sendMessage"
        placeholder="输入消息..."
        rows="3"
        class="message-input"
      ></textarea>
      <button @click="sendMessage" class="send-button" :disabled="isLoading">
        {{ isLoading ? '发送中...' : '发送' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick } from 'vue'
import type { ChatMessage } from '../types/chat'
import { sendMessage as apiSendMessage } from '../api/chat'

const messages = ref<ChatMessage[]>([])
const currentMessage = ref('')
const isLoading = ref(false)
const messagesContainer = ref<HTMLElement | null>(null)

async function sendMessage() {
  if (!currentMessage.value.trim() || isLoading.value) return

  const userMessage: ChatMessage = {
    role: 'user',
    content: currentMessage.value.trim()
  }

  messages.value.push(userMessage)
  currentMessage.value = ''
  isLoading.value = true

  try {
    const response = await apiSendMessage(messages.value)
    messages.value.push({
      role: 'assistant',
      content: response
    })
  } catch (error) {
    console.error('Failed to get response:', error)
  } finally {
    isLoading.value = false
    await nextTick()
    scrollToBottom()
  }
}

function scrollToBottom() {
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}
</script>

<style scoped>
.chat-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: #f5f5f5;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.message {
  max-width: 80%;
  padding: 12px;
  border-radius: 12px;
  margin: 4px 0;
}

.user-message {
  align-self: flex-end;
  background-color: #007AFF;
  color: white;
}

.bot-message {
  align-self: flex-start;
  background-color: white;
  color: #333;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.message-header {
  font-size: 0.8em;
  margin-bottom: 4px;
  opacity: 0.7;
}

.input-container {
  padding: 20px;
  background-color: white;
  border-top: 1px solid #eee;
  display: flex;
  gap: 10px;
}

.message-input {
  flex: 1;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  resize: none;
  font-family: inherit;
}

.send-button {
  padding: 0 20px;
  background-color: #007AFF;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.send-button:hover {
  background-color: #0056b3;
}
</style>
