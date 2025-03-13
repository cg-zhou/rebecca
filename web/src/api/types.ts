
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
