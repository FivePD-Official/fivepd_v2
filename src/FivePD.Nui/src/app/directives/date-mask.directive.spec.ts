import { DateMaskDirective } from './date-mask.directive';
import { FormControl, NgControl } from "@angular/forms";

describe('DateMaskDirective', () => {
  let control: NgControl;

  beforeEach(() => {
    control = new FormControl('') as unknown as NgControl;
  });

  it('should create an instance', () => {
    const directive = new DateMaskDirective(control);
    expect(directive).toBeTruthy();
  });
});
