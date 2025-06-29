<template>
  <div class="home">
    <h1>首页</h1>
    <p>欢迎使用Rebecca</p>
    <nav class="nav-cards">
      <router-link v-for="route in routes" 
                   :key="route.name"
                   :to="{ name: route.name }"
                   class="card"
                   :class="{ active: $route.name === route.name }">
        <h3>{{ route.title }}</h3>
        <p>{{ route.description }}</p>
      </router-link>
    </nav>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const routes = ref([
  {
    path: '/settings',
    name: 'settings',
    title: '设置',
    description: '系统配置选项'
  },
  {
    path: '/about',
    name: 'about',
    title: '关于',
    description: '了解更多信息'
  }
])
</script>

<style scoped>
.home {
  padding: 20px;
}

.nav-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-top: 30px;
}

.card {
  background-color: var(--el-bg-color);
  border-radius: 8px;
  padding: 20px 20px 20px 24px;
  text-decoration: none;
  color: var(--el-text-color-primary);
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  position: relative;
  border: 1px solid var(--el-border-color-light);
}

.card.active {
  color: var(--el-color-primary);
  padding-left: 24px;
  font-weight: 500;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.15);
}

.card.active::after {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 70%;
  background-color: var(--el-color-primary);
  border-radius: 2px;
}

.card:hover {
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.12);
}

.card h3 {
  margin: 0 0 10px 0;
  font-size: 1.2em;
  transition: color 0.3s ease;
}

.card p {
  margin: 0;
  font-size: 0.9em;
  opacity: 0.8;
  transition: opacity 0.3s ease;
}

.card.active p {
  opacity: 0.9;
}
</style>
