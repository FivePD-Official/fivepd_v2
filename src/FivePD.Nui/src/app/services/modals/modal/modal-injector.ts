import { InjectionToken, InjectOptions, Injector, Type } from '@angular/core';

export class ModalInjector implements Injector {

  constructor(
    private _parentInjector: Injector,
    private _locallyResolvableValues: Map<any, any>,
  ) { }

  get<T>(token: Type<T> | InjectionToken<T>, notFoundValue?: T, flags?: InjectOptions): T;
  get(token: any, notFoundValue?: any): any;
  get(token: any, notFoundValue?: any, flags?: any): any {
    const value = this._locallyResolvableValues.get(token);
    if (value) {
      return value;
    }
    return this._parentInjector.get<any>(token, notFoundValue, flags);
  }
}
