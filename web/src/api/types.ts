// 文件夹相关类型
 export interface OpenFolderRequest {
     path: string;
 }
 
 export interface ApiResponse<T = any> {
     success: boolean;
     message?: string;
     data?: T;
 }
 
 // 快捷键相关类型
 export enum HotkeyAction {
     VolumeUp = "VolumeUp",
     VolumeDown = "VolumeDown",
 }
 
 export interface HotkeyConfig {
     actionId: HotkeyAction;
     key: string;
     modifiers: HotkeyModifiers;
 }
 
 export enum HotkeyModifiers {
     None = 0,
     Alt = 1,
     Control = 2,
     Shift = 4,
     Windows = 8,
 }