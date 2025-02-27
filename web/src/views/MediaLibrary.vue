<template>
  <div class="media-library">
    <el-card class="config-card">
      <template #header>
        <div class="card-header">
          <span>媒体库设置</span>
          <div class="header-buttons">
            <el-button type="primary" @click="saveConfig" :disabled="isScanning">保存设置</el-button>
            <el-button type="success" @click="startScan" :loading="isScanning" :disabled="!hasLibraryPaths">
              {{ isScanning ? '扫描中...' : '开始扫描' }}
            </el-button>
            <el-button type="danger" @click="cancelScan" v-if="isScanning">取消</el-button>
          </div>
        </div>
      </template>

      <div class="paths-config">
        <div class="path-list">
          <div v-for="(path, index) in config.libraryPaths" :key="index" class="path-item">
            <el-input v-model="config.libraryPaths[index]" placeholder="请输入媒体库路径">
              <template #append>
                <div class="button-group">
                  <el-button @click="selectFolder(index)">选择</el-button>
                  <el-button @click="removePath(index)">删除</el-button>
                </div>
              </template>
            </el-input>
          </div>
          <div v-if="config.libraryPaths.length === 0" class="empty-paths">
            <el-empty description="没有配置媒体库路径" />
          </div>
        </div>
        <div class="actions">
          <el-button @click="addPath">添加路径</el-button>
        </div>

        <el-divider>高级设置</el-divider>

        <div class="advanced-settings">
          <el-form label-position="left" label-width="140px">
            <el-form-item label="自动扫描">
              <el-switch v-model="config.autoScan" />
            </el-form-item>
            <el-form-item label="扫描间隔 (分钟)" v-if="config.autoScan">
              <el-input-number v-model="config.scanIntervalMinutes" :min="10" :max="1440" />
            </el-form-item>
            <el-form-item label="启用通知">
              <el-switch v-model="config.enableNotifications" />
            </el-form-item>
          </el-form>
        </div>
      </div>
    </el-card>

    <el-card class="status-card">
      <template #header>
        <div class="card-header">
          <span>媒体文件</span>
          <el-button @click="refreshMediaFiles">刷新</el-button>
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
      <div class="empty-files" v-if="mediaFiles.length === 0 && !isLoading">
        <el-empty description="没有媒体文件" />
      </div>
    </el-card>

    <!-- 详情对话框 -->
    <el-dialog v-model="detailsVisible" title="媒体文件详情" width="700px" destroy-on-close>
      <div class="media-details" v-if="selectedFile">
        <div class="media-poster">
          <img :src="getPosterUrl(selectedFile)" alt="海报" class="poster-image" />
        </div>
        <div class="media-info">
          <h2>{{ selectedFile.title }} <span v-if="selectedFile.year">({{ selectedFile.year }})</span></h2>
          <div class="file-path">{{ selectedFile.path }}</div>
          <div class="last-scanned">最后扫描: {{ formatDate(selectedFile.lastScanned) }}</div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'

// 媒体库配置
const config = reactive({
  libraryPaths: [] as string[],
  autoScan: false,
  scanIntervalMinutes: 60,
  enableNotifications: true
})

// 媒体文件列表
const mediaFiles = ref<any[]>([])
const isScanning = ref(false)
const isLoading = ref(false)

// 详情对话框
const detailsVisible = ref(false)
const selectedFile = ref(null)

// 计算属性：是否有媒体库路径
const hasLibraryPaths = computed(() => {
  return config.libraryPaths.length > 0 && config.libraryPaths.some(path => path.trim() !== '')
})

// 生命周期钩子
onMounted(async () => {
  await loadConfig()
  await refreshMediaFiles()
  await checkScanStatus()

  // 定期检查扫描状态
  setInterval(checkScanStatus, 3000)
})

// 加载配置
const loadConfig = async () => {
  try {
    const response = await fetch('/api/medialibrary/config')
    const data = await response.json()

    if (data) {
      config.libraryPaths = data.libraryPaths || []
      config.autoScan = data.autoScan ?? false
      config.scanIntervalMinutes = data.scanIntervalMinutes || 60
      config.enableNotifications = data.enableNotifications ?? true
    }
  } catch (error) {
    console.error('加载配置失败:', error)
    showError('加载配置失败')
  }
}

// 保存配置
const saveConfig = async () => {
  try {
    const response = await fetch('/api/medialibrary/config', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(config)
    })

    if (response.ok) {
      showSuccess('配置保存成功')
    } else {
      showError('配置保存失败')
    }
  } catch (error) {
    console.error('保存配置失败:', error)
    showError('保存配置失败')
  }
}

// 添加路径
const addPath = () => {
  config.libraryPaths.push('')
}

// 移除路径
const removePath = (index: number) => {
  config.libraryPaths.splice(index, 1)
}

// 选择文件夹
const selectFolder = async (index: number) => {
  try {
    const response = await fetch('/api/folder')
    const result = await response.json()
    if (result.success) {
      config.libraryPaths[index] = result.path
    }
  } catch (error) {
    console.error('选择文件夹失败:', error)
    showError('选择文件夹失败')
  }
}

// 刷新媒体文件列表
const refreshMediaFiles = async () => {
  isLoading.value = true
  try {
    const response = await fetch('/api/medialibrary/files')
    const data = await response.json()
    mediaFiles.value = data || []
  } catch (error) {
    console.error('获取媒体文件失败:', error)
    showError('获取媒体文件失败')
  } finally {
    isLoading.value = false
  }
}

// 检查扫描状态
const checkScanStatus = async () => {
  try {
    const response = await fetch('/api/medialibrary/scan/status')
    const data = await response.json()
    isScanning.value = data.isScanning
    
    // 如果扫描状态发生变化，刷新媒体文件列表
    if (isScanning.value === false) {
      await refreshMediaFiles()
    }
  } catch (error) {
    console.error('获取扫描状态失败:', error)
  }
}

// 开始扫描
const startScan = async () => {
  // 先保存配置
  await saveConfig()
  
  try {
    const response = await fetch('/api/medialibrary/scan', {
      method: 'POST'
    })

    if (response.ok) {
      showSuccess('扫描已启动')
      isScanning.value = true
    } else {
      const error = await response.json()
      showError(error.message || '启动扫描失败')
    }
  } catch (error) {
    console.error('启动扫描失败:', error)
    showError('启动扫描失败')
  }
}

// 取消扫描
const cancelScan = async () => {
  try {
    const response = await fetch('/api/medialibrary/scan/cancel', {
      method: 'POST'
    })

    if (response.ok) {
      showSuccess('扫描已取消')
    } else {
      showError('取消扫描失败')
    }
  } catch (error) {
    console.error('取消扫描失败:', error)
    showError('取消扫描失败')
  }
}

// 查看详情
const viewDetails = (file: any) => {
  selectedFile.value = file
  detailsVisible.value = true
}

// 重新处理文件
const reprocess = async (file: any) => {
  // TODO: 实现重新处理单个文件的功能
  showInfo('此功能尚未实现')
}

// 获取海报URL
const getPosterUrl = (file: any) => {
  if (file.posterPath) {
    return `file://${file.posterPath}`
  }
  return '/images/no-poster.png'
}

// 格式化日期
const formatDate = (dateString: string | null) => {
  if (!dateString) return '未知'
  
  try {
    const date = new Date(dateString)
    return date.toLocaleString()
  } catch {
    return dateString
  }
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

// 提示消息
const showSuccess = (message: string) => {
  ElMessage.success(message)
}

const showError = (message: string) => {
  ElMessage.error(message)
}

const showInfo = (message: string) => {
  ElMessage.info(message)
}
</script>

<style scoped>
.media-library {
  padding: 1rem;
}

.config-card, .status-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-buttons {
  display: flex;
  gap: 8px;
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

.button-group {
  display: flex;
  gap: 1px;
}

.button-group .el-button {
  border-left: 1px solid var(--el-input-border-color);
  margin: 0;
}

.button-group .el-button:first-child {
  border-left: none;
}

.advanced-settings {
  margin-top: 1rem;
}

.empty-paths, .empty-files {
  margin: 2rem 0;
}

.media-details {
  display: flex;
  gap: 20px;
}

.media-poster {
  flex: 0 0 200px;
}

.poster-image {
  width: 100%;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.media-info {
  flex: 1;
}

.file-path {
  margin-top: 10px;
  color: #606266;
  font-size: 14px;
  word-break: break-all;
}

.last-scanned {
  margin-top: 5px;
  color: #909399;
  font-size: 13px;
}
</style>
