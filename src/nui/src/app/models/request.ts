export interface BaseRequest {
  Page: number;
  Filters?: Record<string, string>;
}