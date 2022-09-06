import { Injectable } from '@angular/core';
import { filter, Observable, Subject } from 'rxjs';
import { FormInputBase, Localization, Paths } from '@models';

interface ModalButton {
  theme: 'primary' | 'secondary' | 'danger';
  label: Paths<Localization>;
}

export interface ModalOptions {
  title: Paths<Localization>;
  description?: string;
  fields?: FormInputBase<any>[];
  buttons: {
    cancel: ModalButton;
    submit: ModalButton;
  };
}

export interface ModalResponse {
  success: boolean;
  value?: { [key: string]: any; };
}

class Modal {

  title: ModalOptions['title'];
  description?: ModalOptions['description'];
  fields?: ModalOptions['fields'];
  buttons: ModalOptions['buttons'];

  private response$: Observable<ModalResponse>;

  constructor(modal: ModalOptions, modalResponse: Observable<ModalResponse>) {
    this.title = modal.title;
    this.description = modal.description;
    this.fields = modal.fields;
    this.buttons = modal.buttons;
    this.response$ = modalResponse;
  }

  onSuccess(handler: (response: ModalResponse) => void): Modal {
    this.response$.pipe(
      filter(response => response.success),
    ).subscribe(response => handler(response))
    return this;
  }

  onCancel(handler: (response: ModalResponse) => void): Modal {
    this.response$.pipe(
      filter(response => !response.success),
    ).subscribe(response => handler(response))
    return this;
  }
}

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  public readonly modal$ = new Subject<Modal | null>();
  private _modalResponse$ = new Subject<ModalResponse>();

  public open(inputObj: ModalOptions): Modal {
    this._modalResponse$ = new Subject<ModalResponse>();
    const modal = new Modal(inputObj, this._modalResponse$.asObservable());
    this.modal$.next(modal);
    return modal;
  }

  public close(): void {
    this.modal$.next(null);
    this._modalResponse$.next({
      success: false
    });
  }

  public submit(value?: object): void {
    this.modal$.next(null);
    const data: ModalResponse = {
      success: true,
    };
    if (value) {
      data.value = value;
    }
    this._modalResponse$.next(data);
  }

  onOpen(): Observable<Modal | null> {
    return this.modal$.asObservable();
  }
}
