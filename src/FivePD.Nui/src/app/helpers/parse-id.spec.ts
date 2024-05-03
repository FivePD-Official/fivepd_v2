import { parseId } from "./parse-id";

describe('parse-id', () => {

  describe('parseId(id)', () => {

    describe('when called with a string', () => {
      it('should return the correct number', () => {
        expect(parseId('123')).toEqual(123);
      });
    });

    describe('when called with a number', () => {
      it('should return the correct number', () => {
        expect(parseId(123)).toEqual(123);
      });
    });

  });

});
