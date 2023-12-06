import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { DropdownHeightVariants, DropdownItem, DropdownVariants, FormInputBase } from '@models';
import { LocalizationService } from '@services/localization/localization.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { SafeHtmlPipe } from '../../pipes/safe-html.pipe';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';
import { DateMaskDirective } from '../../directives/date-mask.directive';
import { NgClass, NgFor, NgIf, NgSwitch, NgSwitchCase } from '@angular/common';
import { CustomValidators } from "../../helpers/validators";

const dropdownSelectedItemVariants: Record<DropdownVariants, string> = {
  'secondary': `border border-zinc-600 bg-transparent
                before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                before:bg-transparent`,
  'tertiary': `border border-zinc-800
               before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
               before:bg-zinc-800`,
  'tertiary-bg': `border border-zinc-700
                  before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                  before:bg-zinc-700`,
};
const dropdownSelectedItemVariantsHover: Record<DropdownVariants, string> = {
  'secondary': `hover:border-zinc-400`,
  'tertiary': `hover:before:bg-zinc-700`,
  'tertiary-bg': `hover:before:bg-zinc-600`,
};
const dropdownSelectedItemVariantsHoverReadonly: Record<DropdownVariants, string> = {
  'secondary': `hover:border-zinc-600`,
  'tertiary': `hover:before:border-zinc-800`,
  'tertiary-bg': `hover:before:border-zinc-700`,
};

const dropdownHeightVariants: Record<DropdownHeightVariants, string> = {
  'base': 'h-[40px]',
  'sm': 'h-[35px]',
};

@Component({
  selector: 'app-dynamic-form-input',
  templateUrl: './dynamic-form-input.component.html',
  styleUrls: ['./dynamic-form-input.component.sass'],
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgClass, NgSwitch, NgSwitchCase, DateMaskDirective, ClickOutsideDirective, NgFor, SafeHtmlPipe, TranslatePipe]
})
export class DynamicFormInputComponent implements OnInit, OnChanges {

  @Input() form?: FormGroup;
  @Input() field!: FormInputBase<any>;

  @Output() onChange = new EventEmitter();

  multiSelect: boolean = false;
  label: string = '';
  placeholder: string = '';
  helpLabel: string = '';
  pseudoFormControl = new FormControl('');

  dropdownOpen: boolean = false;
  dropdownSelectedItem: DropdownItem = {key: -1, label: ''};
  dropdownSelectedItems: DropdownItem[] = [];
  dropdownFilteredItems: DropdownItem[] = [];
  dropdownSelectedItemLabel: string = '';

  constructor(
    private _localization: LocalizationService,
  ) {
  }

  ngOnInit(): void {
    if (!this.form) {
      this.form = new FormGroup({
        [this.field.key]: this.pseudoFormControl,
      });

      this.control.setValue(this.field.value);
      if (this.field.dropdownOptions) {
        const item = this.field.dropdownOptions.items.find(x => x.key === this.field.value);
        if (item) {
          this.dropdownSelectedItem = item;
        }
      }
    }

    this.setLabels();
    this.setDropdownOptions(this.field);

    this.control?.valueChanges?.subscribe(value => {
      if (this.field.type === 'dropdown') {
        const selectItem = this.dropdownFilteredItems.find(item => item.key === value);
        if (selectItem) {
          //this.onDropdownSelect(selectItem, false);
        }
      }

      if (this.field.type === 'text' || this.field.type === 'textarea') {
        this.onChange.emit(value);
      }
    });

    if (this.field.type === 'dropdown') {
      if (this.control.value === undefined && this.field.value === undefined) {
        this.control?.patchValue(-1);
      }
      if (Array.isArray(this.control.value) && !this.control.value.length) {
        this.control?.patchValue([]);
      }
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    // this.setDropdownOptions(changes['field']?.currentValue as any); // TODO: why was this needed? it fucks up everything
  }

  get show() {
    const show = this.field.show(this.form!);

    let validator: (control: AbstractControl) => ValidationErrors | null;
    if (this.field.type === 'dropdown') {
      if (this.field.dropdownOptions?.type === 'single') {
        validator = CustomValidators.dropdownSingleRequired;
      } else {
        validator = CustomValidators.dropdownMultiRequired;
      }
    } else {
      validator = Validators.required;
    }

    if (show && this.field.required && validator) {
      if (!this.control.hasValidator(validator)) {
        this.control.addValidators(validator);
        this.control.updateValueAndValidity();
      }
    } else {
      if (this.control.hasValidator(validator)) {
        this.control.removeValidators(validator);
        this.control.updateValueAndValidity();
      }
    }

    return show;
  }

  get control() {
    return this.form?.controls[this.field.key] ?? this.pseudoFormControl;
  }

  get dropdownSelectedItemClassName() {
    if (!this.field.dropdownOptions) {
      return {};
    }

    const className = {
      [dropdownSelectedItemVariants[this.field.dropdownOptions?.variant]]: true,
      [dropdownSelectedItemVariantsHover[this.field.dropdownOptions?.variant]]: !this.field.readonly,
      [dropdownSelectedItemVariantsHoverReadonly[this.field.dropdownOptions?.variant]]: this.field.readonly,
    };

    if (this.field.dropdownOptions?.height) {
      className[dropdownHeightVariants[this.field.dropdownOptions?.height]] = true;
    } else {
      className[dropdownHeightVariants['base']] = true;
    }

    return className;
  }

  setDropdownOptions(field?: FormInputBase<any>): void {
    if (!field || !field.dropdownOptions) {
      return;
    }

    const {items, label} = field.dropdownOptions;
    this.dropdownFilteredItems = items;
    this.filterDropdownItems();

    if (field.dropdownOptions.type === 'single' && 'selectedItemResolver' in field.dropdownOptions) {
      const selectedItemResolver = field.dropdownOptions.selectedItemResolver;
      if (selectedItemResolver) {
        const item = items.find(item => selectedItemResolver(item.key));
        if (item) {
          this.setSelectedItem(item, true);
        } else {
          this.dropdownSelectedItem.label = label;
        }
      } else {
        this.dropdownSelectedItem.label = label;
      }
    } else if (field.dropdownOptions.type === 'multi') {
      this.multiSelect = true;
      this.control?.setValue([] as any);
      if ('selectedItemsResolver' in field.dropdownOptions) {
        const selectedItemsResolver = field.dropdownOptions.selectedItemsResolver;
        if (selectedItemsResolver) {
          const selectedItems = items.filter(item => selectedItemsResolver(item.key));
          if (selectedItems.length) {
            this.control?.setValue(selectedItems.map(x => x.key));
            this.dropdownSelectedItems = selectedItems.map(x => ({key: x.key, label: x.label}));
          } else {
            this.dropdownSelectedItem.label = label;
          }
        } else {
          this.dropdownSelectedItem.label = label;
        }
      }
    } else if (this.field.value === undefined) {
      this.control?.patchValue(-1);
      this.dropdownSelectedItem.label = label;
    }

    this.setMultiSelectLabel();
  }

  toggleDropdown(): void {
    if (this.field.readonly) {
      return;
    }

    this.dropdownOpen = !this.dropdownOpen;
  }

  onDropdownSelect(selectItem: DropdownItem, updateControl: boolean = true): void {
    this.dropdownOpen = false;

    if (this.field.dropdownOptions?.type === 'single') {
      this.setSelectedItem(selectItem, updateControl);
    } else if (this.field.dropdownOptions?.type === 'multi') {
      this.setSelectedItems(selectItem, updateControl);
    } else {
      this.setSelectedItem(selectItem, updateControl);
    }
  }

  private setMultiSelectLabel(): void {
    if (!this.field.dropdownOptions) {
      return;
    }

    if (this.dropdownSelectedItems.length) {
      this.dropdownSelectedItemLabel = this._localization.translate('UI.LABEL.SELECTED', {count: this.dropdownSelectedItems.length.toString()}, false);
      return;
    }

    if (this.dropdownSelectedItem) {
      this.dropdownSelectedItemLabel = this.dropdownSelectedItem.label;
      return;
    }

    this.dropdownSelectedItemLabel = this.field.dropdownOptions.label;
  }

  private setSelectedItem(selectedItem: DropdownItem, updateControl: boolean = false): void {
    if (!this.field.dropdownOptions) {
      return;
    }

    if (updateControl) {
      this.control?.patchValue(selectedItem.key);
    } else {
      this.onChange.emit(selectedItem.key);
    }

    this.dropdownSelectedItem = selectedItem;
    this.filterDropdownItems();

    const addDefaultItem = this.field.dropdownOptions.addDefaultItem === undefined || this.field.dropdownOptions.addDefaultItem;
    if (addDefaultItem && selectedItem.key !== -1) {
      this.dropdownFilteredItems.unshift({key: -1, label: this.field.dropdownOptions.label});
    }

    this.setMultiSelectLabel();
    this.updateFormState(updateControl);
  }

  private setSelectedItems(selectedItem: DropdownItem, updateControl: boolean = false): void {
    if (!this.field.dropdownOptions) {
      return;
    }

    let value = this.form?.controls[this.field.key].value;
    const valueIndex = value.indexOf(selectedItem.key);
    if (valueIndex > -1) {
      value.splice(valueIndex, 1);
    } else {
      value.push(selectedItem.key);
    }

    const valueIndex2 = this.dropdownSelectedItems.findIndex(x => x.key === selectedItem.key);
    if (valueIndex2 > -1) {
      this.dropdownSelectedItems.splice(valueIndex2, 1);
    } else {
      this.dropdownSelectedItems.push(selectedItem);
    }

    if (updateControl) {
      this.control?.setValue(value);
    }

    this.onChange.emit(this.dropdownSelectedItems.map(x => x.key));

    this.setMultiSelectLabel();
    this.filterDropdownItems();
    this.updateFormState(updateControl);
  }

  private updateFormState(isCalledAtInit: boolean): void {
    if (isCalledAtInit) {
      return;
    }

    const control = this.form?.controls[this.field.key];
    if (control?.value === this.field.value) {
      control?.markAsPristine();
    } else {
      control?.markAsDirty();
    }
  }

  isItemSelected(item: DropdownItem) {
    return this.dropdownSelectedItems.some(x => x.key === item.key);
  }

  private filterDropdownItems(): void {
    if (this.field.dropdownOptions?.items) {
      this.dropdownFilteredItems = this.field.dropdownOptions.items.filter(item => item.key !== this.dropdownSelectedItem.key);
    }
  }

  private setLabels(): void {
    this.field.label = this._localization.translate(this.field.label);
    this.label = this.field.label;
    if (this.field.required) {
      this.label = `${this.field.label}*`;
    }

    this.helpLabel = this._localization.translate(this.field.helpLabel);
    this.placeholder = this._localization.translate(this.field.placeholder);
  }

}
