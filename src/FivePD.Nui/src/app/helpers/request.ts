import { Collation } from "@models";

export function collationWithAdditionalFilters<T>(collation: Collation, additionalFilters: T): Collation<T> {
  return {
    ...collation,
    Filters: {
      ...collation.Filters,
      ...additionalFilters,
    },
  };
}
