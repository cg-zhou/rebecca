<template>
  <el-dialog v-model="dialogVisible" title="媒体文件详情" width="700px" destroy-on-close>
    <div class="media-details" v-if="media">
      <div class="media-poster">
        <img :src="getPosterUrl(media)" alt="海报" class="poster-image" />
      </div>
      <div class="media-info">
        <h2>{{ media.title }} <span v-if="media.year">({{ media.year }})</span></h2>
        <div class="file-path">{{ media.path }}</div>
        <div class="last-scanned">最后扫描: {{ formatDate(media.lastScanned) }}</div>
      </div>
    </div>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { MediaFile } from '../types'

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
