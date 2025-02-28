<template>
  <div class="media-files">
    <el-card class="media-card">
      <template #header>
        <div class="card-header">
          <span>媒体文件</span>
          <div class="header-buttons">
            <el-button @click="refreshMediaFiles">刷新</el-button>
            <el-button type="success" @click="startScan" :loading="isScanning">
              {{ isScanning ? '扫描中...' : '开始扫描' }}
            </el-button>
            <el-button type="danger" @click="cancelScan" v-if="isScanning">取消</el-button>
          </div>
        </div>
      </template>

      <el-table :data="mediaFiles" style="width: 100%" v-loading="isLoading">
        <el-table-column prop="fileName" label="文件名" />
        <el-table-column prop="title" label="标题">
          <template #default="scope">
            {{ scope.row.title || '未识别' }}
          </template>
        </el-table-column>
        <el-table-column prop="year" label="年份" width="100" />
        <el-table-column prop="status" label="状态" width="120">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.status)">
              {{ getStatusText(scope.row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="scope">
            <el-button type="primary" size="small" @click="viewDetails(scope.row)" v-if="scope.row.status === 'completed'">
              查看
            </el-button>
            <el-button type="warning" size="small" @click="reprocess(scope.row)" v-if="scope.row.status === 'error'">
              重试
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <MediaDetails v-model="detailsVisible" :media="selectedFile" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import MediaDetails from './MediaDetails.vue'
import type { MediaFile } from '../types'

const mediaFiles = ref<MediaFile[]>([])
const isLoading = ref(false)
const isScanning = ref(false)
const detailsVisible = ref(false)
const selectedFile = ref<MediaFile | null>(null)

onMounted(async () => {
  await refreshMediaFiles()
  setInterval(checkScanStatus, 3000)
})

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

const refreshMediaFiles = async () => {
  isLoading.value = true
  try {
    const response = await fetch('/api/medialibrary/files')
    const data = await response.json()
    mediaFiles.value = data || []
  } catch (error) {
    console.error('获取媒体文件失败:', error)
    ElMessage.error('获取媒体文件失败')
  } finally {
    isLoading.value = false
  }
}

const viewDetails = (file: MediaFile) => {
  selectedFile.value = file
  detailsVisible.value = true
}

const reprocess = async (file: MediaFile) => {
  try {
    await fetch(`/api/medialibrary/files/${encodeURIComponent(file.path)}/reprocess`, {
      method: 'POST'
    })
    ElMessage.success('重新处理已开始')
    await refreshMediaFiles()
  } catch (error) {
    console.error('重新处理失败:', error)
    ElMessage.error('重新处理失败')
  }
}

const startScan = async () => {
  try {
    const response = await fetch('/api/medialibrary/scan', { method: 'POST' })
    if (response.ok) {
      ElMessage.success('扫描已启动')
      isScanning.value = true
    } else {
      throw new Error('启动扫描失败')
    }
  } catch (error) {
    console.error('启动扫描失败:', error)
    ElMessage.error('启动扫描失败')
  }
}

const cancelScan = async () => {
  try {
    const response = await fetch('/api/medialibrary/scan/cancel', { method: 'POST' })
    if (response.ok) {
      ElMessage.success('扫描已取消')
    } else {
      throw new Error('取消扫描失败')
    }
  } catch (error) {
    console.error('取消扫描失败:', error)
    ElMessage.error('取消扫描失败')
  }
}

const checkScanStatus = async () => {
  try {
    const response = await fetch('/api/medialibrary/scan/status')
    const data = await response.json()
    isScanning.value = data.isScanning
    if (!isScanning.value) {
      await refreshMediaFiles()
    }
  } catch (error) {
    console.error('获取扫描状态失败:', error)
  }
}
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-buttons {
  display: flex;
  gap: 8px;
}
</style>
