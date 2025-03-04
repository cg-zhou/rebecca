export type MediaFile = {
    id: string;
    path: string;
    fileName: string;
    title?: string;
    year?: number;
    status: 'pending' | 'scanning' | 'downloading' | 'completed' | 'error';
    processingComponent?: ProcessingComponent;
    errorMessage?: string;
    posterPath?: string;
    fanartPath?: string;
    nfoPath?: string;
    hasPoster: boolean;
    hasFanart: boolean;
    hasNfo: boolean;
    isMetadataComplete: boolean;
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

// 与后端对应的处理组件枚举
export enum ProcessingComponent {
    Scanning = 0,
    Nfo = 1,
    Poster = 2,
    Fanart = 3,
    None = 4
}

// 转换组件值到字符串的函数
export function getProcessingComponentName(component: ProcessingComponent | undefined): string {
    if (component === undefined) return 'none';
    
    switch (component) {
        case ProcessingComponent.Scanning:
            return 'scanning';
        case ProcessingComponent.Nfo:
            return 'nfo';
        case ProcessingComponent.Poster:
            return 'poster';
        case ProcessingComponent.Fanart:
            return 'fanart';
        default:
            return 'none';
    }
}
