export interface WebSocketMessage {
    type: string;
    data: any;
}

export interface WebSocketScanStatus {
    isScanning: boolean;
}

export interface WebSocketErrorMessage {
    message: string;
}

export const WebSocketEventType = {
    ScanStatus: 'scanStatus',
    FileStatus: 'fileStatus',
    Error: 'error'
} as const;