<template>
  <div class="media-files">
    <el-card class="media-card">
      <template #header>
        <div class="card-header">
          <span>媒体文件</span>
          <div class="header-buttons">
            <el-button @click="refreshMediaFiles">刷新</el-button>
            <el-button type="success" @click="startScan" :disabled="isScanning">
              {{ isScanning ? '扫描中' : '开始全部扫描' }}
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
        <el-table-column prop="year" label="年份" width="80" />
        
        <!-- NFO状态列 -->
        <el-table-column label="NFO" width="60">
          <template #default="scope">
            <template v-if="isNfoProcessing(scope.row)">
              <el-icon class="is-loading"><el-icon-loading /></el-icon>
            </template>
            <template v-else>
              <el-icon :color="scope.row.hasNfo ? '#67C23A' : '#F56C6C'">
                <el-icon-check v-if="scope.row.hasNfo" />
                <el-icon-close v-else />
              </el-icon>
            </template>
          </template>
        </el-table-column>

        <!-- 海报状态列 -->
        <el-table-column label="海报" width="60">
          <template #default="scope">
            <template v-if="isPosterProcessing(scope.row)">
              <el-icon class="is-loading"><el-icon-loading /></el-icon>
            </template>
            <template v-else>
              <el-icon :color="scope.row.hasPoster ? '#67C23A' : '#F56C6C'">
                <el-icon-check v-if="scope.row.hasPoster" />
                <el-icon-close v-else />
              </el-icon>
            </template>
          </template>
        </el-table-column>

        <!-- 背景图状态列 -->
        <el-table-column label="背景图" width="60">
          <template #default="scope">
            <template v-if="isFanartProcessing(scope.row)">
              <el-icon class="is-loading"><el-icon-loading /></el-icon>
            </template>
            <template v-else>
              <el-icon :color="scope.row.hasFanart ? '#67C23A' : '#F56C6C'">
                <el-icon-check v-if="scope.row.hasFanart" />
                <el-icon-close v-else />
              </el-icon>
            </template>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="状态" width="120">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.status)">
              {{ getStatusText(scope.row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140">
          <template #default="scope">
            <el-button type="primary" size="small" @click="viewDetails(scope.row)" 
                      v-if="scope.row.title && !isFileProcessing(scope.row.path)">
              查看
            </el-button>
            <el-button type="warning" size="small" @click="processFile(scope.row)"
                      :disabled="isFileProcessing(scope.row.path) || (scope.row.status === 'completed' && scope.row.isMetadataComplete)"
                      v-if="!isFileProcessing(scope.row.path)">
              {{ getProcessButtonText(scope.row) }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <MediaDetails v-model="detailsVisible" :media="selectedFile" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Check as ElIconCheck, Close as ElIconClose, Loading as ElIconLoading } from '@element-plus/icons-vue'
import MediaDetails from './MediaDetails.vue'
import type { MediaFile } from '@/views/mediaLibrary/types'
import type { WebSocketScanStatus, WebSocketErrorMessage } from '@/types/websocket'
import { WebSocketEventType } from '@/types/websocket'
import { mediaLibraryApi } from '@/api/api'
import { webSocketService } from '@/services/websocketInstance'
import { ProcessingComponent, getProcessingComponentName } from '@/views/mediaLibrary/types'

const mediaFiles = ref<MediaFile[]>([])
const isLoading = ref(false)
const isScanning = ref(false)
const detailsVisible = ref(false)
const selectedFile = ref<MediaFile | null>(null)
// 跟踪正在处理的文件列表
const processingFiles = ref<string[]>([])
// 用于定期检查扫描状态
let statusCheckInterval: number | null = null

// 处理扫描状态变更
const handleScanStatus = (data: WebSocketScanStatus) => {
  console.log('收到扫描状态更新:', data)
  isScanning.value = data.isScanning
  if (!data.isScanning) {
    refreshMediaFiles()
  }
}

// 处理文件状态变更
const handleFileStatus = (file: MediaFile) => {
  // 如果文件状态变为已完成或错误，从处理列表中移除
  if (file.status === 'completed' || file.status === 'error') {
    const index = processingFiles.value.indexOf(file.path)
    if (index >= 0) {
      processingFiles.value.splice(index, 1)
    }
  }

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

// 从后端查询当前扫描状态
const checkScanStatus = async () => {
  try {
    const status = await mediaLibraryApi.getScanStatus()
    if (isScanning.value !== status.isScanning) {
      console.log('扫描状态通过API更新:', status.isScanning)
      isScanning.value = status.isScanning
      if (!status.isScanning) {
        await refreshMediaFiles()
      }
    }
  } catch (error) {
    console.error('获取扫描状态失败:', error)
  }
}

// 设置定期检查扫描状态的计时器
const startStatusCheckInterval = () => {
  // 如果已经有计时器，先清除
  if (statusCheckInterval !== null) {
    clearInterval(statusCheckInterval)
  }
  
  // 每5秒检查一次扫描状态
  statusCheckInterval = window.setInterval(checkScanStatus, 5000)
}

// 清除状态检查计时器
const clearStatusCheckInterval = () => {
  if (statusCheckInterval !== null) {
    clearInterval(statusCheckInterval)
    statusCheckInterval = null
  }
}

// 观察isScanning的变化，以调整状态检查逻辑
watch(isScanning, (newValue) => {
  if (newValue) {
    // 如果开始扫描，启用状态检查
    startStatusCheckInterval()
  } else {
    // 如果扫描结束，停止状态检查
    clearStatusCheckInterval()
  }
})

onMounted(async () => {
  await refreshMediaFiles()
  
  // 获取初始扫描状态
  try {
    const status = await mediaLibraryApi.getScanStatus()
    isScanning.value = status.isScanning
    if (status.isScanning) {
      // 如果初始化时已经在扫描，启用状态检查
      startStatusCheckInterval()
    }
  } catch (error) {
    console.error('获取扫描状态失败:', error)
  }
  
  // 订阅 WebSocket 消息
  webSocketService.subscribe<WebSocketScanStatus>(WebSocketEventType.ScanStatus, handleScanStatus)
  webSocketService.subscribe<MediaFile>(WebSocketEventType.FileStatus, handleFileStatus)
  webSocketService.subscribe<WebSocketErrorMessage>(WebSocketEventType.Error, handleError)
})

onUnmounted(() => {
  // 清除定时器
  clearStatusCheckInterval()
  
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

// 处理或重新处理单个文件
const processFile = async (file: MediaFile) => {
  // 防止重复处理
  if (processingFiles.value.includes(file.path)) {
    return
  }
  
  // 添加到处理中列表
  processingFiles.value.push(file.path)
  
  try {
    await mediaLibraryApi.processFile(file.path)
    ElMessage.success('文件处理已开始')
  } catch (error) {
    console.error('处理文件失败:', error)
    ElMessage.error('处理文件失败')
    // 从处理中列表移除
    const index = processingFiles.value.indexOf(file.path)
    if (index >= 0) {
      processingFiles.value.splice(index, 1)
    }
  }
}

// 判断文件是否正在处理中
const isFileProcessing = (path: string): boolean => {
  const file = mediaFiles.value.find(f => f.path === path);
  return processingFiles.value.includes(path) || 
         (file?.status === 'scanning' || file?.status === 'downloading');
}

// 判断各个元数据项是否正在处理
const isNfoProcessing = (file: MediaFile): boolean => {
  return file.processingComponent === ProcessingComponent.Nfo || 
         file.processingComponent === ProcessingComponent.Scanning;
}

const isPosterProcessing = (file: MediaFile): boolean => {
  return file.processingComponent === ProcessingComponent.Poster;
}

const isFanartProcessing = (file: MediaFile): boolean => {
  return file.processingComponent === ProcessingComponent.Fanart;
}

// 获取处理按钮文本
const getProcessButtonText = (file: MediaFile): string => {
  if (file.status === 'error') {
    return '重试';
  }
  
  if (file.isMetadataComplete) {
    return '已完整';
  }
  
  // 如果正在处理中，显示"处理中"
  if (isFileProcessing(file.path)) {
    return '处理中';
  }
  
  // 计算缺失的元数据数量
  const missing = [!file.hasNfo, !file.hasPoster, !file.hasFanart].filter(Boolean).length;
  return missing > 0 ? `缺失${missing}项` : '处理';
}

const startScan = async () => {
  try {
    await mediaLibraryApi.startScan()
    ElMessage.success('扫描已启动')
    isScanning.value = true
    // 启动状态检查，防止WebSocket消息丢失
    startStatusCheckInterval()
  } catch (error) {
    console.error('启动扫描失败:', error)
    ElMessage.error('启动扫描失败')
  }
}

const cancelScan = async () => {
  try {
    await mediaLibraryApi.cancelScan()
    ElMessage.success('扫描已取消')
    // 立即检查状态以更新UI
    await checkScanStatus()
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

.is-loading {
  animation: rotating 2s linear infinite;
  font-size: 16px;  /* 调整加载图标大小 */
  color: #409EFF;   /* 使用 Element Plus 的主题蓝色 */
}

.el-icon {
  font-size: 16px;  /* 统一所有图标大小 */
  vertical-align: middle;
}

@keyframes rotating {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
