import { AsyncValidatorFn, FormGroup, ValidatorFn } from '@angular/forms';
import { Paths } from './_dev/nested-object-keys';
import { ActionResponse } from './nui-event-responses';
import { ButtonVariants } from "./button";

export interface FormSubmitEvent {
  value: object;
  markAsPristine: () => void;
}

export interface DropdownItem {
  key: any;
  label: string;
}

export type DropdownVariants = Extract<ButtonVariants, 'secondary' | 'tertiary' | 'tertiary-bg'>;

export type DropdownHeightVariants = 'base' | 'sm';

export interface DropdownBaseOptions {
  label: string;
  addDefaultItem?: boolean;
  items: DropdownItem[];
  size: 'small' | 'normal';
  height?: DropdownHeightVariants;
  variant: DropdownVariants;
  type?: 'single' | 'multi';
}

export interface DropdownSingleOptions {
  selectedItemResolver?: (dropdownItemKey: any) => boolean;
}

export interface DropdownMultiOptions {
  selectedItemsResolver: (dropdownItemKey: any) => boolean;
  resetWhenSelectedId?: number;
}

export type DropdownOptions = DropdownBaseOptions & (DropdownSingleOptions | DropdownMultiOptions);

interface FormInputBaseModel<T> {
  type: 'text' | 'textarea' | 'number' | 'date' | 'checkbox' | 'dropdown' | 'checkboxGroup';
  value: T | undefined;
  key: string;
  label: string;
  helpLabel: string;
  placeholder: string;
  required: boolean;
  readonly: boolean;
  options: Record<string, any>;
  dropdownOptions: DropdownOptions | undefined;
  validators: ValidatorFn[];
  asyncValidators: AsyncValidatorFn[];
  checkboxGroup: Checkbox[];
  show?: (form: FormGroup) => boolean;
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
  dropdownOptions: FormInputBaseModel<T>['dropdownOptions'];
  validators: FormInputBaseModel<T>['validators'];
  asyncValidators: FormInputBaseModel<T>['asyncValidators'];
  checkboxGroup: Checkbox[];
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
    this.dropdownOptions = options.dropdownOptions || undefined;
    this.options = options.options || {};
    this.validators = options.validators || [];
    this.asyncValidators = options.asyncValidators || [];
    this.checkboxGroup = options.checkboxGroup || [];
    this.show = options.show || (() => true);

    if (this.dropdownOptions) {
      if ('selectedItemResolver' in this.dropdownOptions) {
        this.dropdownOptions.type = 'single';
      } else if ('selectedItemsResolver' in this.dropdownOptions) {
        this.dropdownOptions.type = 'multi';
      }
    }
  }

  public get type() {
    return this._type;
  }
}

export class TextInput extends FormInputBase<string> {
  constructor(options: FormInputBaseOptions<string>) {
    super('text', options);
  }
}

export class Textarea extends FormInputBase<string> {
  constructor(options: FormInputBaseOptions<string>) {
    super('textarea', options);
  }
}

export class NumberInput extends FormInputBase<number> {
  constructor(options: FormInputBaseOptions<number>) {
    super('number', options);
  }
}

export class DateInput extends FormInputBase<Date> {
  constructor(options: FormInputBaseOptions<Date>) {
    super('date', options);
  }
}

export class Dropdown extends FormInputBase<number> {
  constructor(options: FormInputBaseOptions<number>) {
    super('dropdown', options);
  }
}

export class Checkbox extends FormInputBase<boolean> {
  constructor(options: FormInputBaseOptions<boolean>) {
    super('checkbox', options);
  }
}

export class CheckboxGroup extends FormInputBase<null> {
  constructor(options: FormInputBaseOptions<null>) {
    super('checkboxGroup', options);
  }
}
