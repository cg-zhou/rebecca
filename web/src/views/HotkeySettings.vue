<template>
    <div class="hotkey-settings">
        <el-table :data="tableData" style="width: 100%">
            <el-table-column prop="label" label="功能名称" width="180"></el-table-column>
            <el-table-column label="快捷键">
                <template #default="scope">
                    {{ formatHotkey(scope.row.hotkey) }}
                </template>
            </el-table-column>
            <el-table-column label="操作" width="180">
                <template #default="scope">
                    <el-button size="small" @click="openHotkeyDialog(scope.row.hotkey)">设置</el-button>
                    <el-button size="small" type="danger" @click="clearHotkey(scope.row.hotkey.actionId)">清除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <el-dialog
            v-model="dialogVisible"
            title="设置快捷键"
            width="400px"
            @close="resetDialog"
        >
            <el-input
                v-model="displayCurrentEditingHotkey"
                @keydown="handleDialogKeydown"
                placeholder="请按下快捷键"
                readonly
                ref="hotkeyInputRef"
            ></el-input>
            <template #footer>
                <span class="dialog-footer">
                    <el-button @click="dialogVisible = false">取消</el-button>
                    <el-button type="primary" @click="confirmHotkey">确定</el-button>
                </span>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick, type Ref } from 'vue';
import { hotkeyApi } from '@/api/api';
import { HotkeyAction, type HotkeyConfig, HotkeyModifiers } from '@/api/types';
import { ElMessage, ElDialog, ElInput, ElTable, ElTableColumn } from 'element-plus';

const volumeUpHotkey = ref<HotkeyConfig>({
    actionId: HotkeyAction.VolumeUp,
    key: '',
    modifiers: HotkeyModifiers.None,
});

const volumeDownHotkey = ref<HotkeyConfig>({
    actionId: HotkeyAction.VolumeDown,
    key: '',
    modifiers: HotkeyModifiers.None,
});

const dialogVisible = ref(false);
const currentEditingHotkey = ref<HotkeyConfig>({
    actionId: HotkeyAction.VolumeUp, // Default to VolumeUp for initialization
    key: '',
    modifiers: HotkeyModifiers.None,
});
const hotkeyInputRef = ref<InstanceType<typeof ElInput> | null>(null);

const tableData = computed(() => [
    { label: '调大音量', hotkey: volumeUpHotkey.value },
    { label: '调小音量', hotkey: volumeDownHotkey.value },
]);

const displayCurrentEditingHotkey = computed(() => {
    return formatHotkey(currentEditingHotkey.value);
});

const fetchHotkeys = async () => {
    try {
        const fetchedHotkeys = await hotkeyApi.getHotkeys();
        const up = fetchedHotkeys.find(h => h.actionId === HotkeyAction.VolumeUp);
        const down = fetchedHotkeys.find(h => h.actionId === HotkeyAction.VolumeDown);

        if (up) volumeUpHotkey.value = up;
        if (down) volumeDownHotkey.value = down;
    } catch (error) {
        ElMessage.error('获取快捷键失败');
    }
};

const saveHotkey = async (hotkey: HotkeyConfig) => {
    try {
        const savedHotkey = await hotkeyApi.setHotkey(hotkey);
        ElMessage.success('快捷键保存成功');
        // Update the corresponding hotkey ref with the saved data
        if (savedHotkey.actionId === HotkeyAction.VolumeUp) {
            volumeUpHotkey.value = savedHotkey;
        } else if (savedHotkey.actionId === HotkeyAction.VolumeDown) {
            volumeDownHotkey.value = savedHotkey;
        }
    } catch (error) {
        ElMessage.error('保存失败');
    }
};

const clearHotkey = async (actionId: HotkeyAction) => {
    try {
        await hotkeyApi.clearHotkey(actionId);
        ElMessage.success('快捷键已清除');
        // Update the corresponding hotkey ref to reflect cleared state
        if (actionId === HotkeyAction.VolumeUp) {
            volumeUpHotkey.value = { actionId: HotkeyAction.VolumeUp, key: '', modifiers: HotkeyModifiers.None };
        } else if (actionId === HotkeyAction.VolumeDown) {
            volumeDownHotkey.value = { actionId: HotkeyAction.VolumeDown, key: '', modifiers: HotkeyModifiers.None };
        }
    } catch (error) {
        ElMessage.error('清除失败');
    }
};

const openHotkeyDialog = (hotkey: HotkeyConfig) => {
    currentEditingHotkey.value = { ...hotkey }; // Create a copy to avoid direct modification
    dialogVisible.value = true;
    nextTick(() => {
        setTimeout(() => {
            hotkeyInputRef.value?.focus();
        }, 50); // Small delay to ensure element is fully rendered
    });
};

const handleDialogKeydown = (event: Event) => {
    event.preventDefault(); // Prevent default browser actions for the key

    if (!(event instanceof KeyboardEvent)) {
        return; // Only process KeyboardEvents
    }

    // Reset modifiers
    currentEditingHotkey.value.modifiers = HotkeyModifiers.None;

    // Set modifiers based on the event
    if (event.ctrlKey) currentEditingHotkey.value.modifiers |= HotkeyModifiers.Control;
    if (event.altKey) currentEditingHotkey.value.modifiers |= HotkeyModifiers.Alt;
    if (event.shiftKey) currentEditingHotkey.value.modifiers |= HotkeyModifiers.Shift;
    if (event.metaKey) currentEditingHotkey.value.modifiers |= HotkeyModifiers.Windows; // metaKey for Windows key

    // Determine the base key using event.code for physical key identification
    let keyToStore = '';

    if (event.code.startsWith('Key')) {
        keyToStore = event.code.substring(3); // e.g., 'KeyA' -> 'A'
    } else if (event.code.startsWith('Digit')) {
        keyToStore = event.code.substring(5); // e.g., 'Digit2' -> '2'
    } else if (event.code === 'Space') {
        keyToStore = 'Space';
    } else if (event.code === 'ArrowUp') {
        keyToStore = 'Up';
    } else if (event.code === 'ArrowDown') {
        keyToStore = 'Down';
    } else if (event.code === 'ArrowLeft') {
        keyToStore = 'Left';
    } else if (event.code === 'ArrowRight') {
        keyToStore = 'Right';
    } else if (event.code === 'Escape') {
        keyToStore = 'Esc';
    } else if (event.code === 'Backspace') {
        keyToStore = 'Backspace';
    } else if (event.code === 'Tab') {
        keyToStore = 'Tab';
    } else if (event.code === 'Enter') {
        keyToStore = 'Enter';
    } else if (event.code === 'NumpadAdd') {
        keyToStore = 'Numpad +';
    } else if (event.code === 'NumpadSubtract') {
        keyToStore = 'Numpad -';
    } else if (event.code === 'NumpadMultiply') {
        keyToStore = 'Numpad *';
    } else if (event.code === 'NumpadDivide') {
        keyToStore = 'Numpad /';
    } else if (event.code === 'NumpadDecimal') {
        keyToStore = 'Numpad .';
    } else if (event.code.startsWith('Numpad')) {
        keyToStore = event.code.substring(6); // e.g., 'Numpad1' -> '1'
    } else {
        keyToStore = event.key;
    }

    // If only modifier keys are pressed, the key should be empty
    if (['Control', 'Alt', 'Shift', 'Meta'].includes(event.key)) {
        currentEditingHotkey.value.key = '';
    } else {
        currentEditingHotkey.value.key = keyToStore.toUpperCase();
    }
};

const confirmHotkey = async () => {
    // Validation: If key is empty but modifiers are present, it's an invalid hotkey
    if (currentEditingHotkey.value.key === '' && currentEditingHotkey.value.modifiers !== HotkeyModifiers.None) {
        ElMessage.warning('快捷键不能只包含 Ctrl/Alt/Shift，请设置一个普通按键。');
        return;
    }

    await saveHotkey(currentEditingHotkey.value);
    dialogVisible.value = false;
};

const resetDialog = () => {
    currentEditingHotkey.value = {
        actionId: HotkeyAction.VolumeUp, // Reset to a default actionId
        key: '',
        modifiers: HotkeyModifiers.None,
    };
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

onMounted(fetchHotkeys);
</script>

<style scoped>

</style>