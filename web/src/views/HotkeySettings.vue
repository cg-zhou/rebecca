<template>
    <div class="hotkey-settings">
        <h3>音量控制快捷键</h3>

        <div class="hotkey-item">
            <label>调大音量:</label>
            <el-input
                v-model="displayVolumeUpHotkey"
                placeholder="未设置"
                readonly
            ></el-input>
            <el-button type="primary" @click="openHotkeyDialog('volume_up')">设置快捷键</el-button>
            <el-button type="danger" @click="clearHotkey('volume_up')">清除</el-button>
        </div>

        <div class="hotkey-item">
            <label>调小音量:</label>
            <el-input
                v-model="displayVolumeDownHotkey"
                placeholder="未设置"
                readonly
            ></el-input>
            <el-button type="primary" @click="openHotkeyDialog('volume_down')">设置快捷键</el-button>
            <el-button type="danger" @click="clearHotkey('volume_down')">清除</el-button>
        </div>

        <el-dialog
            v-model="dialogVisible"
            title="设置快捷键"
            width="30%"
            @close="resetDialog"
        >
            <p>请按下您想要设置的快捷键组合:</p>
            <el-input
                v-model="displayCurrentEditingHotkey"
                @keydown="handleDialogKeydown"
                placeholder="按下按键"
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
import { ref, onMounted, computed, nextTick } from 'vue';
import { hotkeyApi } from '@/api/api';
import type { HotkeyConfig } from '@/api/types';
import { HotkeyModifiers } from '@/api/types';
import { ElMessage, ElDialog, ElInput } from 'element-plus';

const volumeUpHotkey = ref<HotkeyConfig>({
    key: '',
    modifiers: HotkeyModifiers.None,
    actionId: 'volume_up',
});

const volumeDownHotkey = ref<HotkeyConfig>({
    key: '',
    modifiers: HotkeyModifiers.None,
    actionId: 'volume_down',
});

const dialogVisible = ref(false);
const currentEditingHotkey = ref<HotkeyConfig>({
    key: '',
    modifiers: HotkeyModifiers.None,
    actionId: '',
});
const hotkeyInputRef = ref<InstanceType<typeof ElInput> | null>(null);

const displayVolumeUpHotkey = computed(() => {
    return formatHotkey(volumeUpHotkey.value);
});

const displayVolumeDownHotkey = computed(() => {
    return formatHotkey(volumeDownHotkey.value);
});

const displayCurrentEditingHotkey = computed(() => {
    return formatHotkey(currentEditingHotkey.value);
});

const fetchHotkeys = async () => {
    try {
        const fetchedHotkeys = await hotkeyApi.getHotkeys();
        const up = fetchedHotkeys.find(h => h.actionId === 'volume_up');
        const down = fetchedHotkeys.find(h => h.actionId === 'volume_down');

        if (up) volumeUpHotkey.value = up;
        if (down) volumeDownHotkey.value = down;
    } catch (error) {
        ElMessage.error('获取快捷键失败');
    }
};

const saveHotkey = async (hotkey: HotkeyConfig) => {
    try {
        if (hotkey.key === '' && hotkey.modifiers === HotkeyModifiers.None) {
            // If key and modifiers are empty, it means we want to delete the hotkey
            if (hotkey.id) {
                await hotkeyApi.deleteHotkey(hotkey.id);
                ElMessage.success('快捷键已清除');
                hotkey.id = undefined; // Clear the ID after deletion
            }
        } else {
            if (hotkey.id) {
                await hotkeyApi.updateHotkey(hotkey.id, hotkey);
                ElMessage.success('快捷键更新成功');
            } else {
                const newHotkey = await hotkeyApi.addHotkey(hotkey);
                ElMessage.success('快捷键添加成功');
                hotkey.id = newHotkey.id; // Update the ID for future updates
            }
        }
    } catch (error) {
        ElMessage.error('保存失败');
    }
};

const clearHotkey = async (actionId: string) => {
    let hotkeyToClear: HotkeyConfig;
    if (actionId === 'volume_up') {
        hotkeyToClear = volumeUpHotkey.value;
    } else if (actionId === 'volume_down') {
        hotkeyToClear = volumeDownHotkey.value;
    } else {
        return;
    }

    hotkeyToClear.key = '';
    hotkeyToClear.modifiers = HotkeyModifiers.None;
    await saveHotkey(hotkeyToClear);
};

const openHotkeyDialog = (actionId: string) => {
    // Initialize currentEditingHotkey based on the existing hotkey for the actionId
    let existingHotkey: HotkeyConfig | undefined;
    if (actionId === 'volume_up') {
        existingHotkey = volumeUpHotkey.value;
    } else if (actionId === 'volume_down') {
        existingHotkey = volumeDownHotkey.value;
    }

    if (existingHotkey) {
        currentEditingHotkey.value = { ...existingHotkey }; // Create a copy to avoid direct modification
    } else {
        currentEditingHotkey.value = {
            key: '',
            modifiers: HotkeyModifiers.None,
            actionId: actionId,
        };
    }

    dialogVisible.value = true;
    nextTick(() => {
        hotkeyInputRef.value?.focus();
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
        ElMessage.warning('快捷键不能只包含修饰键，请设置一个普通按键。');
        return;
    }

    // Update the corresponding hotkey ref
    if (currentEditingHotkey.value.actionId === 'volume_up') {
        volumeUpHotkey.value = { ...currentEditingHotkey.value };
    } else if (currentEditingHotkey.value.actionId === 'volume_down') {
        volumeDownHotkey.value = { ...currentEditingHotkey.value };
    }

    await saveHotkey(currentEditingHotkey.value);
    dialogVisible.value = false;
};

const resetDialog = () => {
    currentEditingHotkey.value = {
        key: '',
        modifiers: HotkeyModifiers.None,
        actionId: '',
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
.hotkey-settings {
    padding: 20px;
}

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