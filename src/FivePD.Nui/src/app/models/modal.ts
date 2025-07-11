import {FormInputBase} from './form';
import {ButtonVariants} from './button';
import {ValueResolver} from '@services/modals/modal-form/modal-form-value-resolver';
import {DataResolver} from "@services/modals/modal-form/modal-form-data-resolver";
import { CoreAttributeDropdownItems } from "@helpers";

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

export interface ModalOptions<T1 = any, T2 = any> {
  title: string;
  description?: string;
  columns?: number;
  buttons?: ModalCancelButton & ModalSubmitButton;
}

export interface ModalFormOptions<T1, T2 = {}> extends Omit<ModalOptions<T1, T2>, 'buttons'> {
  fields?: (entity?: T1, additionalData?: T2, coreAttributeDropdownItems?: CoreAttributeDropdownItems) => FormInputBase<any>[];
  valueResolver?: ValueResolver<T1>;
  dataResolver?: DataResolver<T2>;
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
