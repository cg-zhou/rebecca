<template>
  <div class="media-library">
    <el-tabs v-model="activeTab">
      <el-tab-pane label="媒体库" name="library">
        <MediaFiles />
      </el-tab-pane>
      <el-tab-pane label="设置" name="settings">
        <MediaSettings />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import MediaFiles from './components/MediaFiles.vue'
import MediaSettings from './components/MediaSettings.vue'

const activeTab = ref('library')
</script>

<style scoped>
.media-library {
  padding: 1rem;
}
</style>
