import type { OpenFolderRequest, ApiResponse, HotkeyConfig, HotkeyAction } from './types';

const BASE_URL = '/api';

function handleError(error: any): any {
    console.error('API Error:', error);
    if (error instanceof SyntaxError) {
        // Handle cases where the response is not valid JSON (e.g., 204 No Content)
        return { success: false, message: 'Invalid JSON response' };
    }
    return { success: false, message: error.message || 'An unknown error occurred' };
}

export const folderApi = {
    async openFolder(request: OpenFolderRequest): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/folder/open`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(request),
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
};

export const volumeApi = {
    async volumeUp(): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/volume/up`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    async volumeDown(): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/volume/down`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    async toggleMute(): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/volume/mute`, {
                method: 'POST',
            });
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
};

export const settingsApi = {
    async getStartup(): Promise<{ enabled: boolean }> {
        try {
            const response = await fetch(`${BASE_URL}/settings/startup`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

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

// Hotkey APIs
export const hotkeyApi = {
    async getHotkeys(): Promise<HotkeyConfig[]> {
        try {
            const response = await fetch(`${BASE_URL}/hotkey`);
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    async setHotkey(hotkey: HotkeyConfig): Promise<HotkeyConfig> {
        try {
            const response = await fetch(`${BASE_URL}/hotkey/set`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(hotkey),
            });
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },

    async clearHotkey(actionId: HotkeyAction): Promise<ApiResponse> {
        try {
            const response = await fetch(`${BASE_URL}/hotkey/clear/${actionId}`, {
                method: 'DELETE',
            });
            if (response.status === 204) {
                return { success: true, message: 'Cleared successfully' };
            }
            return await response.json();
        } catch (error) {
            return handleError(error);
        }
    },
};