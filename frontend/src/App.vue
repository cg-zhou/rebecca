<template>
  <div class="app">
    <h1>DeepSeek Enchant</h1>
    <p>{{ message }}</p>
    <div class="status">
      Backend Status: {{ backendStatus }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

const message = "Welcome to DeepSeek Enchant!"
const backendStatus = ref('Checking...')

onMounted(async () => {
  try {
    const response = await fetch('/api')
    const data = await response.json()
    backendStatus.value = data.message
  } catch (error) {
    backendStatus.value = 'Error connecting to backend'
  }
})
</script>

<style>
:root {
  font-family: Inter, system-ui, Avenir, Helvetica, Arial, sans-serif;
  line-height: 1.5;
  font-weight: 400;
}

.app {
  max-width: 1280px;
  margin: 0 auto;
  padding: 2rem;
  text-align: center;
}

h1 {
  font-size: 3.2em;
  line-height: 1.1;
}

.status {
  margin-top: 2rem;
  padding: 1rem;
  border-radius: 8px;
  background-color: #f9f9f9;
}
</style>
