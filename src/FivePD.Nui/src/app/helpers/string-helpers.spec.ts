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

    describe('when called with: "{{key(x)}}", {key: "KeyLabel", key(x): "lorem ipsum"}, false', () => {
      it('should return "KeyLabel lorem ipsum"', () => {
        expect(replaceParams('{{key(x)}}', {
          key: 'KeyLabel',
          'key(x)': 'lorem ipsum'
        }, false)).toBe('KeyLabel lorem ipsum');
      });
    });

    describe('when called with: "{{key}} {{anotherKey}}", {key: "value1", anotherKey: "value2"}', () => {
      it('should return "<b>value1</b> <b>value2</b>"', () => {
        expect(replaceParams('{{key}} {{anotherKey}}', {
          key: 'value1',
          anotherKey: 'value2'
        })).toBe('<b>value1</b> <b>value2</b>');
      });
    });

    describe('when called with: "{{key}} {{unknownKey}}", {key: "value"}', () => {
      it('should return "<b>value</b> unknownKey"', () => {
        expect(replaceParams('{{key}} {{unknownKey}}', {key: 'value'})).toBe('<b>value</b> unknownKey');
      });
    });

    describe('when called with: "{{key}}", {}', () => {
      it('should return "key"', () => {
        expect(replaceParams('{{key}}', {})).toBe('key');
      });
    });

    describe('when called with: "Hello {{name}}!", {name: "John"}', () => {
      it('should return "Hello <b>John</b>!"', () => {
        expect(replaceParams('Hello {{name}}!', {name: 'John'})).toBe('Hello <b>John</b>!');
      });
    });

    describe('when called with: "Line 1{{NEW_LINE}}Line 2", {}', () => {
      it('should return "Line 1<br/>Line 2"', () => {
        expect(replaceParams('Line 1{{NEW_LINE}}Line 2', {}, false)).toBe('Line 1<br/>Line 2');
      });
    });

    describe('when called with: "{{key}} {{key(value)}}", {key: "Label", "key(value)": "description"}', () => {
      it('should return "<b>Label</b> <b>Label description</b>"', () => {
        expect(replaceParams('{{key}} {{key(value)}}', {
          key: 'Label',
          'key(value)': 'description'
        })).toBe('<b>Label</b> <b>Label description</b>');
      });
    });

    describe('when called with: "{{key}} {{key(unknown)}}", {key: "Label"}', () => {
      it('should return "<b>Label</b> <b>Label</b>"', () => {
        expect(replaceParams('{{key}} {{key(unknown)}}', {key: 'Label'})).toBe('<b>Label</b> <b>Label</b>');
      });
    });

  });

});
