export interface MediaFile {
    id: string;
    path: string;
    fileName: string;
    status: string;
    lastScanned?: string;  // 修改为 string 类型，因为从后端接收到的是 ISO 日期字符串
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

export interface TmdbConfigRequest {
    bearerToken: string;
    baseApiUrl?: string;
    baseImageUrl?: string;
    language?: string;
    apiKeyType?: 'v3' | 'v4';
}
