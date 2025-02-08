import type { ChatMessage, ChatResponse } from '../types/chat';

const API_BASE_URL = 'http://localhost:8000';

export async function sendMessage(messages: ChatMessage[]): Promise<string> {
    const response = await fetch(`${API_BASE_URL}/chat`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        },
        mode: 'cors',
        body: JSON.stringify({ messages }),
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error || 'Failed to send message');
    }

    const data: ChatResponse = await response.json();
    return data.response;
}
