import { ComponentRef } from '@angular/core';
import { FormInputBase } from './form';
import { ModalComponent } from '../components/modal/modal.component';
import { filter, Observable, Subject } from 'rxjs';
import { ButtonVariants } from './button';

interface ModalButton {
  variant: ButtonVariants;
  label: string;
}

export interface ModalOptions {
  title: string;
  description?: string;
  fields?: FormInputBase<any>[];
  columns?: number;
  buttons?: {
    cancel: ModalButton;
    submit: ModalButton;
  };
}

export interface ModalResponse {
  success: boolean;
  value: Record<string, any>;
}

export class ModalRef {

  private result$ = new Subject<ModalResponse>();
  modalComponentRef!: ComponentRef<ModalComponent>;
  options!: ModalOptions;

  constructor(options: ModalOptions) {
    this.options = options;
  }

  get result(): Observable<ModalResponse> {
    return this.result$;
  }

  onSuccess(handler: (response: ModalResponse) => void): ModalRef {
    this.result$.pipe(
      filter(response => response.success),
    ).subscribe(response => handler(response))
    return this;
  }

  close(result: ModalResponse): void {
    this.result$.next(result);
    this.result$.complete();
  }
}
