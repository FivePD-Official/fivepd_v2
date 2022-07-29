export interface Page<T> {
  CurrentPage: number;
  Items: T[];
  PageCount: number;
  PageSize: number;
  ItemCount: number;
}