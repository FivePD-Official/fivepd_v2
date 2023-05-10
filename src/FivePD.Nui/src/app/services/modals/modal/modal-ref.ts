import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { ComponentRef } from '@angular/core';
import { ModalComponent } from '../../../components/modal/modal.component';
import { ModalLoader, ModalOptions, ModalResponse } from '@models';

export class ModalRef<T = any> {

  private readonly result$ = new Subject<ModalResponse>();
  private readonly loader$ = new BehaviorSubject<ModalLoader | undefined>(undefined);
  public modalComponentRef!: ComponentRef<ModalComponent>;
  public readonly options: ModalOptions<T>;
  private _loader?: ModalLoader;
  private _loading = false;

  constructor(options: ModalOptions<T>) {
    this.options = options;
  }

  get result(): Observable<ModalResponse> {
    return this.result$;
  }

  get loader(): Observable<ModalLoader | undefined> {
    return this.loader$;
  }

  close(result: ModalResponse): void {
    this.result$.next(result);
  }

  startLoading(label: string): void {
    this._loader = {label};
    this._loading = true;
    this.loader$.next(this._loader);
  }

  stopLoading(): void {
    this._loading = false;
    this.loader$.next(undefined);
  }
}
