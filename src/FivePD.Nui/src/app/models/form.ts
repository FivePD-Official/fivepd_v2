import { AsyncValidatorFn, FormGroup, ValidatorFn } from '@angular/forms';
import { Paths } from './_dev/nested-object-keys';
import { ActionResponse } from './nui-event-responses';
import { ButtonVariants } from "./button";

export interface FormSubmitEvent {
  value: any;
  markAsPristine: () => void;
}

export interface FormChangeEvent {
  value: any;
  valid: boolean;
}

export interface DropdownItem {
  key: any;
  label: string;
}

export type DropdownVariants = Extract<ButtonVariants, 'secondary' | 'tertiary' | 'tertiary-bg'>;

export type DropdownHeightVariants = 'base' | 'sm';

export interface DropdownBaseOptions {
  defaultItemLabel: string;
  addDefaultItem?: boolean;
  items: DropdownItem[];
  size: 'small' | 'normal';
  height?: DropdownHeightVariants;
  variant: DropdownVariants;
}

export interface SingleSelectFieldOptions extends DropdownBaseOptions {
}

export interface MultiSelectFieldOptions extends DropdownBaseOptions {
  resetWhenSelectedId?: number;
}

interface FormInputBaseModel<T> {
  type: 'text' | 'textarea' | 'number' | 'date' | 'checkbox' | 'singleSelect' | 'multiSelect' | 'checkboxGroup';
  value: T | undefined;
  key: string;
  label: string;
  helpLabel: string;
  placeholder: string;
  required: boolean;
  readonly: boolean;
  validators: ValidatorFn[];
  asyncValidators: AsyncValidatorFn[];
  show?: (form: FormGroup) => boolean;
  options: Record<string, any>;
}

export type FormInputBaseOptions<T> = {
  [P in keyof Omit<FormInputBaseModel<T>, 'type'>]?: FormInputBaseModel<T>[P];
};

export abstract class FormInputBase<T> {

  private readonly _type: FormInputBaseModel<T>['type'];
  value: FormInputBaseModel<T>['value'];
  key: FormInputBaseModel<T>['key'];
  label: FormInputBaseModel<T>['label'];
  helpLabel: FormInputBaseModel<T>['helpLabel'];
  placeholder: FormInputBaseModel<T>['placeholder'];
  required: FormInputBaseModel<T>['required'];
  readonly: FormInputBaseModel<T>['readonly'];
  options: FormInputBaseModel<T>['options'];
  validators: FormInputBaseModel<T>['validators'];
  asyncValidators: FormInputBaseModel<T>['asyncValidators'];
  show: (form: FormGroup) => boolean;

  protected constructor(
    type: FormInputBaseModel<T>['type'],
    options: FormInputBaseOptions<T> = {},
  ) {
    this._type = type;
    this.value = options.value;
    this.key = options.key || '';
    this.label = options.label || '';
    this.helpLabel = options.helpLabel || '';
    this.placeholder = options.placeholder || '';
    this.required = !!options.required;
    this.readonly = !!options.readonly;
    this.options = options.options || {};
    this.validators = options.validators || [];
    this.asyncValidators = options.asyncValidators || [];
    this.show = options.show || (() => true);
  }

  public get type() {
    return this._type;
  }
}

export class TextField extends FormInputBase<string> {
  constructor(options: FormInputBaseOptions<string>) {
    super('text', options);
  }
}

export class TextareaField extends FormInputBase<string> {
  constructor(options: FormInputBaseOptions<string>) {
    super('textarea', options);
  }
}

export class NumberField extends FormInputBase<number> {
  constructor(options: FormInputBaseOptions<number>) {
    super('number', options);
  }
}

export class DateField extends FormInputBase<Date> {
  constructor(options: FormInputBaseOptions<Date>) {
    super('date', options);
  }
}

export class SingleSelectField extends FormInputBase<number> {
  defaultItemLabel: string;
  addDefaultItem?: boolean;
  items: DropdownItem[];
  size: 'small' | 'normal';
  height?: DropdownHeightVariants;
  variant: DropdownVariants;

  constructor(options: FormInputBaseOptions<number> & SingleSelectFieldOptions) {
    super('singleSelect', options);
    this.defaultItemLabel = options.defaultItemLabel;
    this.addDefaultItem = options.addDefaultItem;
    this.items = options.items;
    this.size = options.size;
    this.height = options.height;
    this.variant = options.variant;
  }
}

export class MultiSelectField extends FormInputBase<number[]> {
  defaultItemLabel: string;
  addDefaultItem?: boolean;
  items: DropdownItem[];
  size: 'small' | 'normal';
  height?: DropdownHeightVariants;
  variant: DropdownVariants;
  resetWhenSelectedId?: number;

  constructor(options: FormInputBaseOptions<number[]> & MultiSelectFieldOptions) {
    super('multiSelect', options);
    this.defaultItemLabel = options.defaultItemLabel;
    this.addDefaultItem = options.addDefaultItem;
    this.items = options.items;
    this.size = options.size;
    this.height = options.height;
    this.variant = options.variant;
    this.resetWhenSelectedId = options.resetWhenSelectedId;
  }
}

export class CheckboxField extends FormInputBase<boolean> {
  constructor(options: FormInputBaseOptions<boolean>) {
    super('checkbox', options);
  }
}
