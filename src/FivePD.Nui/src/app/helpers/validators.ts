import { AsyncValidatorFn, ValidatorFn, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, first, map, Observable, switchMap } from 'rxjs';
import { NameTakenResponse } from '@models';

export class CustomValidators {

  static onlyLetters(): ValidatorFn {
    return Validators.pattern('^[a-zA-Z]+$');
  }

  static callsignFormat(): ValidatorFn {
    return Validators.pattern('^[A-Z0-9-*]+');
  }

  static date(): ValidatorFn {
    return c => {
      // @ts-ignore
      if (c.value instanceof Date && !isNaN(c.value)) {
        return null;
      }

      return {invalidDate: true};
    };
  }

  static nameShouldNotBeTaken(nameValidatorFn: (name: string) => Observable<NameTakenResponse>): AsyncValidatorFn {
    return control => {
      if (!control?.valueChanges) {
        return Promise.resolve(null);
      }

      return control.valueChanges.pipe(
        debounceTime(500),
        distinctUntilChanged(),
        switchMap(value => nameValidatorFn(value)),
        map(unique => unique.Taken ? {nameTaken: true} : null),
        first(),
      );
    }
  }

}
