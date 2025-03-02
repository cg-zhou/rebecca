export interface MediaFile {
    id: string;
    path: string;
    fileName: string;
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

export interface TmdbConfigRequest {
    bearerToken: string;
    baseApiUrl?: string;
    baseImageUrl?: string;
    language?: string;
    apiKeyType?: 'v3' | 'v4';
}
