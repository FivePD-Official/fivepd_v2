import { Injectable } from '@angular/core';
import { AsyncValidatorFn } from '@angular/forms';
import { debounceTime, distinctUntilChanged, first, map, Observable, switchMap } from 'rxjs';
import { NameTakenResponse } from '@models';

@Injectable()
export class GenericNameValidator {

  static get(nameValidatorFn: (name: string) => Observable<NameTakenResponse>): AsyncValidatorFn {
    return control => control.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(value => nameValidatorFn(value)),
      map(unique => unique.Taken ? { 'nameTaken': true } : null),
      first(),
    );
  }
}
