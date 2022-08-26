import { AsyncValidatorFn, ValidatorFn } from '@angular/forms';
import { Paths } from './_dev/nested-object-keys';
import { Localization } from './localization';

export interface FormSubmitEvent {
  value: object;
  markAsPristine: () => void;
}

export interface DropdownItem {
  key: number;
  label: string;
}

interface FormInputBaseModel<T> {
  type: 'text' | 'number' | 'checkbox' | 'dropdown' | 'checkboxGroup';
  value: T | undefined;
  key: string;
  label: Paths<Localization> | string;
  helpLabel: Paths<Localization> | string;
  placeholder: Paths<Localization> | string;
  required: boolean;
  readonly: boolean;
  options: Record<string, any>;
  dropdownOptions: {
    label: string;
    selectedItemResolver?: (dropdownItemKey: number) => boolean;
    items: DropdownItem[];
    size: 'small' | 'normal';
  } | null;
  validators: ValidatorFn[];
  asyncValidators: AsyncValidatorFn[];
  checkboxGroup: Checkbox[];
}

type FormInputBaseOptions<T> = {
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
    this.dropdownOptions = options.dropdownOptions || null;
    this.options = options.options || {};
    this.validators = options.validators || [];
    this.asyncValidators = options.asyncValidators || [];
    this.checkboxGroup = options.checkboxGroup || [];
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

export class NumberInput extends FormInputBase<number> {
  constructor(options: FormInputBaseOptions<number>) {
    super('number', options);
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
