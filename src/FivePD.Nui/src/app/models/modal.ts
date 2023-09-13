import { FormInputBase } from './form';
import { ButtonVariants } from './button';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';

interface ModalButton {
  variant: ButtonVariants;
  label: string;
}

export interface ModalCancelButton {
  cancel: ModalButton;
}

interface ModalSubmitButton {
  submit: ModalButton;
}

export interface ModalOptions<T = any> {
  title: string;
  description?: string;
  columns?: number;
  buttons?: ModalCancelButton & ModalSubmitButton;
}

export interface ModalFormOptions<T> extends Omit<ModalOptions<T>, 'buttons'> {
  fields?: (entity: T) => FormInputBase<any>[];
  valueResolver?: ValueResolver<T>;
  // TODO: add some "additionalValueResolver" and pass its returned value to the "fields" as the second param, usage for example in messages component -> getAllSimple user
  buttons: ModalSubmitButton;
}

export interface ModalConfirmOptions<T> extends NonNullable<Omit<ModalOptions<T>, 'columns' | 'buttons'>> {
  buttons: ModalSubmitButton;
}

export interface ModalResponse {
  readonly success: boolean;
  readonly value: Record<string, any>;
}

export interface ModalLoader {
  readonly label: string;
}
