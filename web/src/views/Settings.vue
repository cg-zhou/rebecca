<template>
  <div class="settings">
    <div class="setting-item">
      <label for="startup-switch">开机启动</label>
      <el-switch id="startup-switch" v-model="isStartupEnabled" @change="onStartupChange" />
    </div>
    <!-- 可以添加其他设置内容 -->
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { settingsApi } from '@/api/api';
import { ElMessage } from 'element-plus';

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

.setting-item {
  display: flex;
  align-items: center;
  gap: 1rem;
}
</style>
