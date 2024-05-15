export interface Collation<TFilters = Record<string, any>> {
  Page: number;
  Filters: TFilters;
  Sorting?: any; // TODO
}
