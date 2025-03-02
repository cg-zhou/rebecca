import { WebSocketService } from './websocket';

// 使用相对路径，自动适应当前域名和端口
const wsUrl = `${window.location.protocol === 'https:' ? 'wss:' : 'ws:'}//${window.location.host}/ws`;
export const webSocketService = new WebSocketService(wsUrl);