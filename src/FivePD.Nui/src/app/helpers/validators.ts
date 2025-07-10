import { AbstractControl, AsyncValidatorFn, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, first, map, Observable, switchMap } from 'rxjs';
import { NameTakenResponse } from '@models';

export class CustomValidators {

  private static readonly coreAttributeNameRegex = /^(?!.*(UserId|UserIds|DepartmentId|DepartmentIds|PedHistoryId|PedHistoryIds|IncidentHistoryId|IncidentHistoryIds)).*$/;

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

  static notCoreAttributeName(): ValidatorFn {
    return c => {
      const valid = CustomValidators.coreAttributeNameRegex.test(c.value);
      return valid ? null : {notCoreAttributeName: true};
    };
  }

  static dropdownSingleRequired(control: AbstractControl): ValidationErrors | null {
    return control.value === -1 ? {required: true} : null;
  }

  static dropdownMultiRequired(control: AbstractControl): ValidationErrors | null {
    return !Array.isArray(control.value) ? null : control.value?.length === 0 ? {required: true} : null;
  }

  static positionComponentsRequired(): ValidatorFn {
    return c => {
      if (c.value === undefined) {
        return {positionComponentsRequired: true};
      }

      if ([c.value['X'], c.value['Y'], c.value['Z']].some(value => value === null || value === undefined)) {
        return {positionComponentsRequired: true};
      }

      return null;
    }
  }
}
