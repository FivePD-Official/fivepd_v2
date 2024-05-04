import { TestBed } from '@angular/core/testing';
import { MockedWithProperties } from './test-helper.module';
import { ProviderToken } from '@angular/core';
import { MockStore } from './mock-store';

export class TestHelper {

  static injectMock<T>(token: ProviderToken<T>): MockedWithProperties<T> {
    return TestBed.inject(token) as MockedWithProperties<T>;
  }

  // static injectMockStore(): MockStore {
  //   return TestBed.inject(Store) as unknown as MockStore;
  // }
}
