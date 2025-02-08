<template>
  <div class="app">
    <ChatBox />
  </div>
</template>

<script setup lang="ts">
import ChatBox from './components/ChatBox.vue'
</script>

<style>
:root {
  font-family: Inter, system-ui, Avenir, Helvetica, Arial, sans-serif;
  line-height: 1.5;
  font-weight: 400;
}

body {
  margin: 0;
  padding: 0;
  font-family: Arial, sans-serif;
}

.app {
  min-height: 100vh;
  max-width: 1280px;
  margin: 0 auto;
  padding: 2rem;
  text-align: center;
  background-color: #f5f5f5;
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
