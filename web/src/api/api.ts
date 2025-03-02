import type { MediaFile, MediaLibraryConfig, TmdbConfigRequest, OpenFolderRequest, ApiResponse } from './types';

const BASE_URL = '/api';

// 统一处理请求错误
const handleError = (error: any) => {
    console.error('API Error:', error);
    throw error;
};

// MediaLibrary APIs
export const mediaLibraryApi = {
    // 获取配置
    async getConfig(): Promise<MediaLibraryConfig> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/config`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 更新配置
    async updateConfig(config: MediaLibraryConfig): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/config`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(config),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 获取所有媒体文件
    async getMediaFiles(): Promise<MediaFile[]> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/files`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 开始扫描
    async startScan(): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/scan`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 取消扫描
    async cancelScan(): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/scan/cancel`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 获取扫描状态
    async getScanStatus(): Promise<{ isScanning: boolean }> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/scan/status`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 重新处理文件
    async reprocessFile(filePath: string): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/medialibrary/files/${encodeURIComponent(filePath)}/reprocess`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 获取图片
    getImageUrl(imagePath: string): string {
        return `${BASE_URL}/medialibrary/image/${encodeURIComponent(imagePath)}`;
    },
};

// Settings APIs
export const settingsApi = {
    // 获取TMDB配置
    async getTmdbConfig(): Promise<TmdbConfigRequest> {
        try {
            const response = await fetch(`${BASE_URL}/settings/tmdb`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 保存TMDB配置
    async saveTmdbConfig(config: TmdbConfigRequest): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/settings/tmdb`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(config),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    // 测试TMDB API连接
    async testTmdbApi(config: TmdbConfigRequest): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/settings/tmdb/test`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(config),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
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