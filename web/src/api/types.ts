// MediaLibrary 相关类型
export interface MediaFile {
    id: string;
    fileName: string;
    path: string;
    status: string;
    lastScanned?: Date;
    title?: string;
    year?: number;
    posterPath?: string;
    fanartPath?: string;
    nfoPath?: string;
    errorMessage?: string;
    size: number;
}

export interface MediaLibraryConfig {
    libraryPaths: string[];
    tmdbApiKey: string;
    tmdbLanguage: string;
}

// TMDB 相关类型
export interface TmdbConfigRequest {
    bearerToken?: string;
    baseApiUrl?: string;
    baseImageUrl?: string;
    language?: string;
    apiKeyType?: 'v3' | 'v4';
}

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