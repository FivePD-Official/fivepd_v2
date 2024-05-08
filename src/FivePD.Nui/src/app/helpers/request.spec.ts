import { collationWithAdditionalFilters } from "./request";
import { Collation } from "@models";

describe('request', () => {

  describe('collationWithAdditionalFilters(collation, additionalFilters)', () => {

    describe('when called with empty additionalFilters', () => {
      it('should return the collation without any changes ', () => {
        const collation: Collation = {
          Page: 0,
          Filters: {
            A: false,
          },
          Sorting: {},
        };
        expect(collationWithAdditionalFilters(collation, {})).toEqual(collation);
      });
    });

    describe('when called with non-empty additionalFilter', () => {
      it('should return the collation with the new filters added', () => {
        const collation: Collation = {
          Page: 0,
          Filters: {
            A: false,
          },
          Sorting: {},
        };
        expect(collationWithAdditionalFilters(collation, {B: false, C: 123})).toEqual({
          ...collation,
          Filters: {
            ...collation.Filters,
            B: false,
            C: 123,
          },
        });
      });
    });

  });

});
