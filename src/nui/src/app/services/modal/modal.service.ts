import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { FormFields } from '@utils';
import { FormInputBase, InputField } from '@models';

interface ModalButton {
  theme: 'primary' | 'secondary' | 'danger';
  label: string;
}

export interface Modal {
  title: string;
  description?: string;
  fields?: FormInputBase<any>[];
  buttons: {
    cancel: ModalButton;
    submit: ModalButton;
  };
}

export interface ModalReturn {
  success: boolean;
  value?: object;
}

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  public readonly modal$ = new Subject<Modal | null>();
  private _modalResponse$ = new Subject<ModalReturn>();

  public open(inputObj: Modal): Observable<ModalReturn> {
    this.modal$.next(inputObj);
    this._modalResponse$ = new Subject<ModalReturn>();
    return this._modalResponse$.asObservable();
  }

  public close(): void {
    this.modal$.next(null);
    this._modalResponse$.next({
      success: false
    });
  }

  public submit(value?: object): void {
    this.modal$.next(null);
    const data: ModalReturn = {
      success: true,
    };
    if (value) {
      data.value = value;
    }
    this._modalResponse$.next(data);
  }

  public onOpen(): Subject<Modal | null> {
    return this.modal$;
  }
}
