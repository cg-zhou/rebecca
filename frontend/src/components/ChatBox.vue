<template>
  <div class="chat-container">
    <div class="chat-messages" ref="messagesContainer">
      <div 
        v-for="(msg, index) in messages" 
        :key="index"
        :class="['message-wrapper', msg.role === 'user' ? 'user-message' : 'ai-message']"
      >
        <template v-if="msg.role === 'assistant'">
          <div class="avatar-container">
            <img src="../assets/bot-avatar.svg" alt="Bot Avatar" />
          </div>
          <MessageDisplay :content="msg.content" :isUser="false" />
        </template>
        <template v-else>
          <MessageDisplay :content="msg.content" :isUser="true" />
          <div class="avatar-container">
            <img src="../assets/user-avatar.svg" alt="User Avatar" />
          </div>
        </template>
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
import MessageDisplay from './MessageDisplay.vue'

const messages = ref<ChatMessage[]>([])
const currentMessage = ref('写一段python程序')
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
  width: 100vw;
  min-width: 100%;
  display: flex;
  flex-direction: column;
  background-color: #f5f5f5;
  overflow-x: hidden;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

.message-wrapper {
  display: flex;
  align-items: flex-start;
  margin-bottom: 16px;
  width: 100%;
  gap: 8px;
}

.user-message {
  justify-content: flex-end;
}

.ai-message {
  justify-content: flex-start;
}

.avatar-container {
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatar-container img {
  width: 32px;
  height: 32px;
}

.message-wrapper :deep(.message) {
  max-width: calc(70% - 40px);
  border-radius: 4px;
  word-wrap: break-word;
  margin: 0;
}

.user-message :deep(.message) {
  background-color: #E6E1FF;  /* 改为浅紫色背景 */
  color: #333;  /* 改为深色文字 */
  border-radius: 4px;
  margin-left: 8px;
}

.ai-message :deep(.message) {
  background-color: white;
  color: #333;
  border-radius: 4px;
  margin-right: 8px;
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
  border-radius: 4px;
  resize: none;
  font-family: inherit;
}

.send-button {
  padding: 0 20px;
  background-color: #7B68EE;  /* 保持按钮为深紫色 */
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.send-button:hover:not(:disabled) {
  background-color: #6A5ACD;  /* 悬停时颜色稍深 */
}

.send-button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
</style>
