<template>
  <el-dialog v-model="dialogVisible" title="媒体文件详情" width="700px" destroy-on-close>
    <div class="media-details" v-if="media">
      <div class="media-poster">
        <img :src="getPosterUrl(media)" alt="海报" class="poster-image" />
      </div>
      <div class="media-info">
        <h2>{{ media.title }} <span v-if="media.year">({{ media.year }})</span></h2>
        <div class="info-item">
          <label>文件路径：</label>
          <span class="file-path">{{ media.path }}</span>
        </div>
        <div class="info-item">
          <label>最后扫描：</label>
          <span>{{ formatDate(media.lastScanned) }}</span>
        </div>
        <div class="info-item" v-if="media.size">
          <label>文件大小：</label>
          <span>{{ formatFileSize(media.size) }}</span>
        </div>
      </div>
    </div>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { MediaFile } from '@/views/mediaLibrary/types'
import { mediaLibraryApi } from '@/api/api'

const props = defineProps<{
  modelValue: boolean
  media: MediaFile | null
}>()

const emit = defineEmits(['update:modelValue'])

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const getPosterUrl = (file: MediaFile) => {
  if (file.posterPath) {
    return mediaLibraryApi.getImageUrl(file.posterPath)
  }
  return '/images/no-poster.png'
}

const formatDate = (date: Date | undefined) => {
  if (!date) return '未知'
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(2)} ${sizes[i]}`
}
</script>

<style scoped>
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

.info-item {
  margin-top: 12px;
  line-height: 1.5;
}

.info-item label {
  color: #606266;
  font-weight: bold;
  margin-right: 8px;
}

.file-path {
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
