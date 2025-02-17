<script setup lang="ts">
import { House, InfoFilled, Setting, Folder } from '@element-plus/icons-vue'
import { useRoute } from 'vue-router'
import { computed } from 'vue'

const route = useRoute()
const activeMenu = computed(() => route.path)
</script>

<template>
  <el-container class="layout-container">
    <el-aside width="180px">
      <div class="menu-container">
        <el-menu mode="vertical" router class="side-menu" :default-active="activeMenu">
          <div class="top-menu">
            <el-menu-item index="/">
              <el-icon><House /></el-icon>
              <span>首页</span>
            </el-menu-item>
            <el-menu-item index="/media-library">
              <el-icon><Folder /></el-icon>
              <span>媒体库</span>
            </el-menu-item>
          </div>
          <div class="bottom-menu">
            <el-menu-item index="/settings">
              <el-icon><Setting /></el-icon>
              <span>设置</span>
            </el-menu-item>
            <el-menu-item index="/about">
              <el-icon><InfoFilled /></el-icon>
              <span>关于</span>
            </el-menu-item>
          </div>
        </el-menu>
      </div>
    </el-aside>
    
    <el-container>
      <el-main>
        <router-view></router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.layout-container {
  min-height: 100vh;
  margin: 0;
  padding: 0;
}

.el-aside {
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  background-color: var(--el-menu-bg-color);  /* 使用Element Plus的变量 */
  width: 180px !important;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.menu-container {
  height: 100%;
}

.side-menu {
  height: 100%;
  display: flex;
  flex-direction: column;
  background-color: var(--el-menu-bg-color);
}

.top-menu {
  flex-grow: 1;
}

.bottom-menu {
  margin-top: auto;
}

.el-menu-item {
  height: 45px !important;
  line-height: 45px !important;
  color: var(--el-menu-text-color);
  user-select: none;  /* 添加这行，禁止文本选择 */
}

.el-menu {
  --el-menu-bg-color: var(--el-menu-bg-color);
  --el-menu-text-color: var(--el-menu-text-color);
  --el-menu-hover-bg-color: var(--el-menu-item-hover-fill);  /* 修改这行 */
  --el-menu-active-text-color: var(--el-menu-active-text-color);
  border-right: none !important;
  background-color: transparent;
}

.el-menu-item.is-active {
  background-color: var(--el-menu-item-active-fill) !important;  /* 使用新的选中状态背景色 */
  color: var(--el-menu-text-color) !important;
  position: relative;
}

.el-menu-item.is-active::after {
  content: '';
  position: absolute;
  right: 0;
  top: 0;
  width: 3px;
  height: 100%;
  background-color: var(--el-color-primary);
}

.el-main {
  margin-left: 180px;
  padding: 20px;
}

/* 移除 el-header 和 el-footer 相关样式 */
</style>
