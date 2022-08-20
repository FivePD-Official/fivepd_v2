import { AsyncValidatorFn } from '@angular/forms';
import { Observable, of } from 'rxjs';

export class BaseValidator {

  static create(nameValidatorFn: (name: string) => Observable<boolean>): AsyncValidatorFn {
    return () => of(null);
  };
}
