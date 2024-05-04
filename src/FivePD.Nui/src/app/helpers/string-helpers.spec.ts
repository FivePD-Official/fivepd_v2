import { replaceParams } from "./string-helpers";

describe('string-helpers', () => {

  describe('replaceParams(text, params, boldParams)', () => {

    describe('when called with: "lorem ipsum"', () => {
      it('should return "lorem ipsum"', () => {
        expect(replaceParams('lorem ipsum')).toBe('lorem ipsum');
      });
    });

    describe('when called with: "lorem ipsum", {key: "replacedValue"}', () => {
      it('should return "lorem ipsum"', () => {
        expect(replaceParams('lorem ipsum', {key: 'replacedValue'})).toBe('lorem ipsum');
      });
    });

    describe('when called with: "lorem {{key}} ipsum", {key: "replacedValue"}', () => {
      it('should return "lorem <b>replacedValue<b> ipsum', () => {
        expect(replaceParams('lorem {{key}} ipsum', {key: 'replacedValue'})).toBe('lorem <b>replacedValue</b> ipsum');
      });
    });

    describe('when called with: "lorem {{key}} ipsum", {key: "replacedValue"}, true', () => {
      it('should return "lorem <b>replacedValue</b> ipsum"', () => {
        expect(replaceParams('lorem {{key}} ipsum', {key: 'replacedValue'}, true)).toBe('lorem <b>replacedValue</b> ipsum');
      });
    });

    describe('when called with: "lorem {{key}} ipsum", {key: "replacedValue"}, false', () => {
      it('should return "lorem replacedValue ipsum"', () => {
        expect(replaceParams('lorem {{key}} ipsum', {key: 'replacedValue'}, false)).toBe('lorem replacedValue ipsum');
      });
    });

  });

});
