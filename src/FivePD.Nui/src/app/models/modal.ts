import { ComponentRef } from '@angular/core';
import { Paths } from './_dev/nested-object-keys';
import { Localization } from './localization';
import { FormInputBase } from './form';
import { ModalComponent } from '../components/modal/modal.component';
import { filter, Observable, Subject } from 'rxjs';
import { ButtonVariants } from './button';
import { LooseDataEntity, LooseDataEntityWrapper, MappedLooseDataEntity } from './entities';

interface ModalButton {
  variant: ButtonVariants;
  label: Paths<Localization>;
}

export interface ModalOptions {
  title: Paths<Localization>;
  description?: string;
  fields?: (entity?: MappedLooseDataEntity) => FormInputBase<any>[];
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

export class ModalRef {

  private result$ = new Subject<ModalResponse>();
  modalComponentRef!: ComponentRef<ModalComponent>;
  options!: ModalOptions;
  loader?: ModalLoader;
  loading = false;

  constructor(options: ModalOptions) {
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
