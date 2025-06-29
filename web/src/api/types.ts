

// 文件夹相关类型
export interface OpenFolderRequest {
    path: string;
}

// API 响应类型
export interface ApiResponse<T = any> {
    success?: boolean;
    message?: string;
    data?: T;
}

// 快捷键相关类型
export interface HotkeyConfig {
    id?: number;
    key: string;
    modifiers: HotkeyModifiers;
    actionId: string;
}

export enum HotkeyModifiers {
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8,
}

