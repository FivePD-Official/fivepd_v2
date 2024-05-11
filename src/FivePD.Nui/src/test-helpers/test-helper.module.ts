import { HttpClientTestingModule } from '@angular/common/http/testing';
import { InjectionToken, NgModule, Type } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { MockWindowEventListener } from './mock-window-event-listener';
import NonFunctionPropertyNames = jest.NonFunctionPropertyNames;
import Mocked = jest.Mocked;
import { MockStore } from './mock-store';
import { Store } from "@ngrx/store";
import { TranslatePipe } from "../app/pipes/translate.pipe";
import { MockTranslatePipe } from "./mock-translate-pipe";
import { LocalizationService } from "@services/localization/localization.service";
import { RouterTestingModule } from "@angular/router/testing";

interface Nextable<T> {
  next: (value: T) => void;
  complete: () => void;
  error: (error: any) => void;
}

type MockValues<C> = { [K in NonFunctionPropertyNames<C>]: C[K] extends Observable<infer O> ? C[K] & Nextable<O> : C[K] };

export type MockedWithProperties<T> = Mocked<T> & MockValues<T>;

export class MockedObjectProvider {

  private readonly providedObjects = new Map<Type<any> |  InjectionToken<any>, any>();

  get<T>(type: Type<T> | InjectionToken<T>): T {
    return this.providedObjects.get(type);
  }

  getMock<T>(type: Type<T>): T {
    return this.providedObjects.get(type);
  }

  has(type: Type<any>): boolean {
    return this.providedObjects.has(type);
  }

  set<T>(type: Type<T> | InjectionToken<T>, value: T): void {
    this.providedObjects.set(type, value);
  }

  forEach(callbackFn: (value: any, key: Type<any> | InjectionToken<any>, map: Map<Type<any> | InjectionToken<any>, any>) => void, thisArg?: any): void {
    this.providedObjects.forEach(callbackFn);
  }
}

class TestHelperModuleBuilder {

  private readonly providedObjects = new MockedObjectProvider();

  constructor(
    private readonly moduleType: Type<any>
  ) { }

  private hasProvidedObject(key: Type<any>): boolean {
    return this.providedObjects.has(key);
  }

  withRouter() {
    if (this.hasProvidedObject(Router)) {
      return this;
    }

    const router: any = {
      events: new Subject(),
      navigate: jest.fn(),
    };
    this.providedObjects.set(Router, router as unknown as Router);
    return this;
  }

  withWindowEventListener() {
    if (this.hasProvidedObject(MockWindowEventListener)) {
      return this;
    }

    this.providedObjects.set(MockWindowEventListener, new MockWindowEventListener());
    return this;
  }

  withStore() {
    if (this.hasProvidedObject(Store)) {
      return this;
    }

    this.providedObjects.set(Store, new MockStore() as unknown as Store);

    return this;
  }

  withTranslatePipe() {
    if (this.hasProvidedObject(TranslatePipe)) {
      return this;
    }

    this.providedObjects.set(TranslatePipe, new MockTranslatePipe() as TranslatePipe);

    return this;
  }

  withLocalizationService() {
    if (this.hasProvidedObject(LocalizationService)) {
      return this;
    }

    const mockedLocalizationService = {
      load: () => {},
      translate: (path: string) => `translated: ${path}`,
    };

    this.providedObjects.set(LocalizationService, mockedLocalizationService as LocalizationService);

    return this;
  }

  build() {
    const providers: any = [];
    this.providedObjects.forEach((value: any, key: any) => {
      providers.push({
        provide: key,
        useValue: value,
      });
    });

    return {
      ngModule: this.moduleType,
      providers
    };
  }
}

@NgModule({
  imports: [
    HttpClientTestingModule,
    RouterTestingModule,
  ],
  declarations: []
})
export class TestHelperModule {
  static builder() {
    return new TestHelperModuleBuilder(TestHelperModule);
  }
}
