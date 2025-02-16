<template>
  <div class="media-library">
    <el-card class="config-card">
      <template #header>
        <div class="card-header">
          <span>媒体库设置</span>
          <el-button type="primary" @click="startScan">开始扫描</el-button>
        </div>
      </template>

      <div class="paths-config">
        <div class="path-list">
          <div v-for="(_, index) in libraryPaths" :key="index" class="path-item">
            <el-input v-model="libraryPaths[index]" placeholder="请输入媒体库路径">
              <template #append>
                <div class="button-group">
                  <el-button @click="selectFolder(index)">选择</el-button>
                  <el-button @click="removePath(index)">删除</el-button>
                </div>
              </template>
            </el-input>
          </div>
        </div>
        <el-button @click="addPath">添加路径</el-button>
      </div>
    </el-card>

    <el-card class="status-card">
      <template #header>
        <div class="card-header">
          <span>处理状态</span>
        </div>
      </template>

      <el-table :data="mediaFiles" style="width: 100%">
        <el-table-column prop="path" label="文件路径" />
        <el-table-column prop="status" label="状态">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.status)">
              {{ getStatusText(scope.row.status) }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const libraryPaths = ref<string[]>([])
const mediaFiles = ref<any[]>([])

// 添加路径
const addPath = () => {
  libraryPaths.value.push('')
}

// 移除路径
const removePath = (index: number) => {
  libraryPaths.value.splice(index, 1)
}

// 选择文件夹
const selectFolder = async (index: number) => {
  try {
    const response = await fetch('/api/folder/select')
    const result = await response.json()
    if (result.success) {
      libraryPaths.value[index] = result.path
    }
  } catch (error) {
    console.error('选择文件夹失败:', error)
  }
}

// 开始扫描
const startScan = async () => {
  // TODO: 调用后端API开始扫描
}

// 获取状态类型
const getStatusType = (status: string) => {
  const types: Record<string, string> = {
    pending: 'info',
    scanning: 'warning',
    downloading: 'warning',
    completed: 'success',
    error: 'danger'
  }
  return types[status] || 'info'
}

// 获取状态文本
const getStatusText = (status: string) => {
  const texts: Record<string, string> = {
    pending: '待处理',
    scanning: '扫描中',
    downloading: '下载元数据中',
    completed: '已完成',
    error: '错误'
  }
  return texts[status] || status
}
</script>

<style scoped>
.media-library {
  padding: 1rem;
}

.config-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.paths-config {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.path-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.status-card {
  margin-top: 1rem;
}

.button-group {
  display: flex;
  gap: 1px;
}

/* 修复按钮在input append中的边框样式 */
.button-group .el-button {
  border-left: 1px solid var(--el-input-border-color);
  margin: 0;
}
.button-group .el-button:first-child {
  border-left: none;
}
</style>
