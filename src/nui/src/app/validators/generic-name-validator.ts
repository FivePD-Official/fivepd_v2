import { Injectable } from '@angular/core';
import { AsyncValidatorFn } from '@angular/forms';
import { debounceTime, distinctUntilChanged, first, map, Observable, of, switchMap, tap } from 'rxjs';
import { BaseValidator } from './base-validator';

@Injectable()
export class GenericNameValidator extends BaseValidator {

  constructor() {
    super();
  }

  static override create(nameValidatorFn: (name: string) => Observable<boolean>): AsyncValidatorFn {
    return control => control.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(value => nameValidatorFn(value)),
      map((unique: boolean) => unique ? { 'nameTaken': true } : null),
      first(),
    );
  }
}
