import { ComponentRef } from '@angular/core';
import { Paths } from './_dev/nested-object-keys';
import { Localization } from './localization';
import { FormInputBase } from './form';
import { ModalComponent } from '../components/modal/modal.component';
import { BehaviorSubject, map, Observable, Subject, tap } from 'rxjs';
import { ButtonVariants } from './button';

export class ValueResolver<TValue = any, TUpdatedValue = any> {

  private _value: TValue;
  private _subject = new BehaviorSubject<TValue | TUpdatedValue>(this.defaultValue);
  private _updating = false;
  public readonly updating = new BehaviorSubject<boolean>(false);

  constructor(
    private readonly defaultValue: TValue,
    private readonly updater?: Observable<TUpdatedValue>,
    private readonly mapper?: (value: TUpdatedValue) => TValue) {
    this._value = defaultValue;
    this._subject.next(defaultValue);
    this.update();
  }

  update() {
    if (!this.updater) {
      return;
    }

    this._updating = true;
    this.updating.next(this._updating);
    this.updater
      .pipe(
        map(value => this.mapper ? this.mapper(value) : value),
        tap(() => {
          this._updating = false;
          this.updating.next(this._updating);
        })
      ).subscribe(v => {
      this._subject.next(v);
    });
  }

  get value() {
    return this._subject.asObservable()
  }

}

interface ModalButton {
  variant: ButtonVariants;
  label: Paths<Localization>;
}

export interface ModalOptions<T = any> {
  title: Paths<Localization>;
  description?: string;
  fields?: (entity: T) => FormInputBase<any>[];
  valueResolver?: ValueResolver<T>;
  columns?: number;
  buttons?: {
    cancel: ModalButton;
    submit: ModalButton;
  };
}

export interface ModalResponse {
  readonly success: boolean;
  readonly value: Record<string, any>;
}

export interface ModalLoader {
  readonly label: string;
}

export class ModalRef<T = any> {

  private result$ = new Subject<ModalResponse>();
  modalComponentRef!: ComponentRef<ModalComponent>;
  options!: ModalOptions<T>;
  loader?: ModalLoader;
  loading = false;

  constructor(options: ModalOptions<T>) {
    this.options = options;
  }

  get result(): Observable<ModalResponse> {
    return this.result$;
  }

  close(result: ModalResponse): void {
    this.result$.next(result);
  }

  startLoading(label: string): void {
    this.loader = {label};
    this.loading = true;
  }

  stopLoading(): void {
    this.loading = false;
  }
}
