export interface GameNotificationOptions {
  Title: string;
  TitleParams: Record<string, string>;
  Message: string;
  MessageParams: Record<string, string>;
  ImageSrc: string;
  Timeout: number;
  Keys?: Record<string, string>;
  PlayAudio?: string;
}
