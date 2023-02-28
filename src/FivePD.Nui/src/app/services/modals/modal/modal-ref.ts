import { Observable, Subject } from 'rxjs';
import { ComponentRef } from '@angular/core';
import { ModalComponent } from '../../../components/modal/modal.component';
import { ModalLoader, ModalOptions, ModalResponse } from '@models';

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
