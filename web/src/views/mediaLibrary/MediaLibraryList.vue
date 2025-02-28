<template>
  <div class="media-library-list">
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
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'

interface MediaFile {
  fileName: string
  title: string
  year?: string
  path: string
  lastScanned: string | null
  status: string
  posterPath?: string
}

const mediaFiles = ref<MediaFile[]>([])
const isLoading = ref(false)
const detailsVisible = ref(false)
const selectedFile = ref<MediaFile | null>(null)

onMounted(async () => {
  await refreshMediaFiles()
})

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
  ElMessage.info('此功能尚未实现')
}

const getPosterUrl = (file: MediaFile) => {
  if (file.posterPath) {
    return `file://${file.posterPath}`
  }
  return '/images/no-poster.png'
}

const formatDate = (dateString: string | null) => {
  if (!dateString) return '未知'
  try {
    const date = new Date(dateString)
    return date.toLocaleString()
  } catch {
    return dateString
  }
}

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
</script>

<style scoped>
.media-library-list {
  margin-top: 1rem;
}

.status-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.empty-files {
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