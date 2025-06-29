<template>
  <div class="about">
    <el-card class="about-card">
      <template #header>
        <div>
          <span>关于 Rebecca</span>
        </div>
      </template>
      
      <div class="card-content">
        <p class="description">
          Rebecca 是一个开源的工具集合项目，包含一些杂七杂八的小工具。
        </p>

        <div class="links">
          <el-link @click="openUrl(githubUrl)" type="info">
            <el-icon class="link-icon"><platform /></el-icon>代码库
          </el-link>
          <el-divider direction="vertical" />
          <el-link @click="openUrl(authorUrl)" type="info">
            <el-icon class="link-icon"><house /></el-icon>关于作者
          </el-link>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { Platform, House } from '@element-plus/icons-vue'

const githubUrl = 'https://github.com/cg-zhou/rebecca'
const authorUrl = 'https://cg-zhou.top/'

const openUrl = (url: string) => {
  window.open(url, '_blank')
}
</script>

<style scoped>
.about {
  padding: 1rem;
}

.about-card {
  margin-bottom: 20px;
}

.card-content {
  text-align: center;
}

.description {
  color: var(--el-text-color-regular);
  line-height: 1.6;
  margin-bottom: 1rem;
}

.links {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
}

.link-icon {
  margin-right: 4px;
}
</style>