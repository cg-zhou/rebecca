<template>
    <div class="hotkey-item">
        <label>{{ label }}:</label>
        <el-input
            v-model="displayHotkey"
            placeholder="未设置"
            readonly
        ></el-input>
        <el-button type="primary" @click="emitOpenDialog">设置快捷键</el-button>
        <el-button type="danger" @click="clearHotkey">清除</el-button>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { HotkeyConfig } from '@/api/types';
import { HotkeyModifiers } from '@/api/types';
import { ElMessage } from 'element-plus';

const props = defineProps<{ 
    label: string;
    hotkey: HotkeyConfig;
}>();

const emit = defineEmits<{ 
    (e: 'open-dialog', actionId: string): void;
    (e: 'update:hotkey', hotkey: HotkeyConfig): void;
}>();

const displayHotkey = computed(() => {
    return formatHotkey(props.hotkey);
});

const emitOpenDialog = () => {
    emit('open-dialog', props.hotkey.actionId);
};

const clearHotkey = () => {
    const clearedHotkey: HotkeyConfig = {
        ...props.hotkey,
        key: '',
        modifiers: HotkeyModifiers.None,
    };
    emit('update:hotkey', clearedHotkey);
};

const formatHotkey = (hotkey: HotkeyConfig) => {
    const modifiers = [];
    if (hotkey.modifiers & HotkeyModifiers.Control) modifiers.push('Ctrl');
    if (hotkey.modifiers & HotkeyModifiers.Alt) modifiers.push('Alt');
    if (hotkey.modifiers & HotkeyModifiers.Shift) modifiers.push('Shift');
    if (hotkey.modifiers & HotkeyModifiers.Windows) modifiers.push('Win');

    const keyName = hotkey.key ? hotkey.key : '';
    return [...modifiers, keyName].filter(Boolean).join(' + ');
};
</script>

<style scoped>
.hotkey-item {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 15px;
}

.hotkey-item label {
    width: 100px; /* Adjust as needed */
    text-align: right;
}

.hotkey-item .el-input {
    flex-grow: 1;
}
</style>