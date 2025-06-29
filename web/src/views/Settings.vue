<template>
  <div class="settings">
    <el-card class="setting-card">
      <template #header>
        <div>
          <span>通用设置</span>
        </div>
      </template>
      <div class="setting-item">
        <label for="startup-switch">开机启动</label>
        <el-switch id="startup-switch" v-model="isStartupEnabled" @change="onStartupChange" />
      </div>
    </el-card>

    <el-card class="setting-card hotkey-card">
      <template #header>
        <div>
          <span>快捷键管理</span>
        </div>
      </template>
      <HotkeySettings />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { settingsApi } from '@/api/api';
import { ElMessage } from 'element-plus';
import HotkeySettings from './HotkeySettings.vue';

const isStartupEnabled = ref(false);

onMounted(async () => {
  try {
    const response = await settingsApi.getStartup();
    isStartupEnabled.value = response.enabled;
  } catch (error) {
    ElMessage.error('获取开机启动状态失败');
  }
});

const onStartupChange = async (value: boolean) => {
  try {
    await settingsApi.setStartup(value);
    ElMessage.success(`设置成功，${value ? '已开启' : '已关闭'}开机启动`);
  } catch (error) {
    ElMessage.error('设置开机启动失败');
    // 如果设置失败，恢复原来的状态
    isStartupEnabled.value = !value;
  }
};
</script>

<style scoped>
.settings {
  padding: 1rem;
}

.setting-card {
  margin-bottom: 20px;
}

.setting-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}
</style>
