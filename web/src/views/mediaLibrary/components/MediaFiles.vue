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
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import MediaDetails from './MediaDetails.vue'
import type { MediaFile } from '@/views/mediaLibrary/types'
import type { WebSocketScanStatus, WebSocketErrorMessage } from '@/types/websocket'
import { WebSocketEventType } from '@/types/websocket'
import { mediaLibraryApi } from '@/api/api'
import { webSocketService } from '@/services/websocketInstance'

const mediaFiles = ref<MediaFile[]>([])
const isLoading = ref(false)
const isScanning = ref(false)
const detailsVisible = ref(false)
const selectedFile = ref<MediaFile | null>(null)

// 处理扫描状态变更
const handleScanStatus = (data: WebSocketScanStatus) => {
  isScanning.value = data.isScanning
  if (!data.isScanning) {
    refreshMediaFiles()
  }
}

// 处理文件状态变更
const handleFileStatus = (file: MediaFile) => {
  const index = mediaFiles.value.findIndex(f => f.path === file.path)
  if (index >= 0) {
    // 使用数组解构确保视图更新
    mediaFiles.value = [
      ...mediaFiles.value.slice(0, index),
      file,
      ...mediaFiles.value.slice(index + 1)
    ]
  } else {
    // 添加新文件到列表开头
    mediaFiles.value = [file, ...mediaFiles.value]
  }
}

// 处理错误消息
const handleError = (data: WebSocketErrorMessage) => {
  ElMessage.error(data.message)
}

onMounted(async () => {
  await refreshMediaFiles()
  
  // 订阅 WebSocket 消息
  webSocketService.subscribe<WebSocketScanStatus>(WebSocketEventType.ScanStatus, handleScanStatus)
  webSocketService.subscribe<MediaFile>(WebSocketEventType.FileStatus, handleFileStatus)
  webSocketService.subscribe<WebSocketErrorMessage>(WebSocketEventType.Error, handleError)
})

onUnmounted(() => {
  // 取消订阅
  webSocketService.unsubscribe(WebSocketEventType.ScanStatus, handleScanStatus)
  webSocketService.unsubscribe(WebSocketEventType.FileStatus, handleFileStatus)
  webSocketService.unsubscribe(WebSocketEventType.Error, handleError)
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
    mediaFiles.value = await mediaLibraryApi.getMediaFiles()
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
    await mediaLibraryApi.reprocessFile(file.path)
    ElMessage.success('重新处理已开始')
    await refreshMediaFiles()
  } catch (error) {
    console.error('重新处理失败:', error)
    ElMessage.error('重新处理失败')
  }
}

const startScan = async () => {
  try {
    await mediaLibraryApi.startScan()
    ElMessage.success('扫描已启动')
    isScanning.value = true
  } catch (error) {
    console.error('启动扫描失败:', error)
    ElMessage.error('启动扫描失败')
  }
}

const cancelScan = async () => {
  try {
    await mediaLibraryApi.cancelScan()
    ElMessage.success('扫描已取消')
  } catch (error) {
    console.error('取消扫描失败:', error)
    ElMessage.error('取消扫描失败')
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
