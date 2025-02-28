export interface MediaFile {
  fileName: string
  title: string
  year?: string
  path: string
  lastScanned: string | null
  status: string
  posterPath?: string
}

export interface MediaLibraryConfig {
  libraryPaths: string[]
  tmdbApiKey: string
  tmdbLanguage: string
}
