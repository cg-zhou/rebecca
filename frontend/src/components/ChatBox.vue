<template>
  <div class="chat-container">
    <div class="messages" ref="messagesContainer">
      <div v-for="(message, index) in messages" 
           :key="index" 
           :class="['message', message.role]">
        <div class="content">{{ message.content }}</div>
      </div>
    </div>
    <div class="input-container">
      <textarea
        v-model="currentMessage"
        @keydown.enter.prevent="sendMessage"
        placeholder="Type your message here..."
        :disabled="isLoading"
      ></textarea>
      <button @click="sendMessage" :disabled="isLoading">
        {{ isLoading ? 'Sending...' : 'Send' }}
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
  display: flex;
  flex-direction: column;
  height: 100vh;
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.messages {
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
  border-radius: 8px;
  word-wrap: break-word;
}

.message.user {
  align-self: flex-end;
  background-color: #007AFF;
  color: white;
}

.message.assistant {
  align-self: flex-start;
  background-color: #E9ECEF;
  color: black;
}

.input-container {
  display: flex;
  gap: 10px;
  padding: 20px 0;
}

textarea {
  flex: 1;
  height: 60px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  resize: none;
}

button {
  padding: 0 20px;
  background-color: #007AFF;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
</style>
