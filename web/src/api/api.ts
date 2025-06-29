import type { OpenFolderRequest, ApiResponse } from './types';

const BASE_URL = '/api';

// 统一处理请求错误
const handleError = (error: any) => {
    console.error('API Error:', error);
    throw error;
};

// Folder APIs
export const folderApi = {
    // 选择文件夹
    async selectFolder(): Promise<{ success: boolean; path?: string }> {
        try {
            const response = await fetch(`${BASE_URL}/folder/select`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 打开文件夹
    async openFolder(path: string): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/folder/open`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ path } as OpenFolderRequest),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
};

// Settings APIs
export const settingsApi = {
    // 获取开机启动状态
    async getStartup(): Promise<{ enabled: boolean }> {
        try {
            const response = await fetch(`${BASE_URL}/settings/startup`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 设置开机启动
    async setStartup(enabled: boolean): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/settings/startup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ enabled }),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
};
