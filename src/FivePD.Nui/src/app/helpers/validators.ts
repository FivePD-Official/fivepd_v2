import { AsyncValidatorFn, ValidatorFn, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, first, map, Observable, switchMap } from 'rxjs';
import { NameTakenResponse } from '@models';

export class CustomValidators {

  static callsign(): ValidatorFn {
    return Validators.pattern('^[a-zA-Z]+$');
  }

  static callsignFormat(): ValidatorFn {
    return Validators.pattern('^[NL0-9-*]{1,10}$');
  }

  static nameShouldNotBeTaken(nameValidatorFn: (name: string) => Observable<NameTakenResponse>): AsyncValidatorFn {
    return control => control.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(value => nameValidatorFn(value)),
      map(unique => unique.Taken ? {'nameTaken': true} : null),
      first(),
    );
  }

}
