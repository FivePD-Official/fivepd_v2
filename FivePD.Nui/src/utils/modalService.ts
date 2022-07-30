import { Subject } from 'rxjs';
import { InputField } from "@interfaces";
import { FormFields } from "@utils";

interface ModalButton {
  theme: 'primary' | 'secondary' | 'danger';
  label: string;
}

export interface Modal {
  title: string;
  description?: string;
  buttons: {
    cancel: ModalButton;
    submit: ModalButton;
  };
  input?: InputField;
}

export interface ModalReturn {
  success: boolean;
  inputFields?: FormFields,
}

class ModalService {

  public readonly modal$ = new Subject<Modal | null>();
  private _modalResponse$ = new Subject<ModalReturn>();

  public open(inputObj: Modal): Subject<ModalReturn> {
    this.modal$.next(inputObj);
    this._modalResponse$ = new Subject<ModalReturn>();
    return this._modalResponse$;
  }

  public close(): void {
    this.modal$.next(null);
    this._modalResponse$.next({
      success: false
    });
  }
  
  public submit(fields?: FormFields): void {
    this.modal$.next(null);
    const data: ModalReturn = {
      success: true,
    };
    if (fields) {
      data.inputFields = fields;
    }
    this._modalResponse$.next(data);
  }
  
  public onOpen(): Subject<Modal | null> {
    return this.modal$;
  }

}

export const modalService = new ModalService();