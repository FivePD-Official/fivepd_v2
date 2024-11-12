export interface GameNotificationOptions {
  Title: string;
  Message: string;
  MessageParams: Record<string, string>;
  ImageSrc: string;
  Timeout: number;
  Keys?: Record<string, string>;
  PlayAudio?: string;
}
