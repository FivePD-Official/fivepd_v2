import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import {
  ButtonVariants, DropdownHeightVariants,
  DropdownItem,
  DropdownMultiOptions,
  DropdownSingleOptions,
  DropdownVariants,
  FormInputBase
} from '@models';
import { LocalizationService } from '@services/localization/localization.service';

const dropdownSelectedItemVariants: Record<DropdownVariants, string> = {
  'secondary': `border border-zinc-600 hover:border-zinc-400 bg-transparent
                before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                before:bg-transparent`,
  'tertiary': `border border-zinc-800
               before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
               before:bg-zinc-800 hover:before:bg-zinc-700`,
  'tertiary-bg': `border border-zinc-700
                  before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                  before:bg-zinc-700 hover:before:bg-zinc-600`,
};

const dropdownHeightVariants: Record<DropdownHeightVariants, string> = {
  'base': 'h-[40px]',
  'sm': 'h-[35px]',
};

@Component({
  selector: 'app-dynamic-form-input',
  templateUrl: './dynamic-form-input.component.html',
  styleUrls: ['./dynamic-form-input.component.sass']
})
export class DynamicFormInputComponent implements OnInit {

  @Input() withoutForm: boolean = false;
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
  multiSelectLabel: string = '';

  constructor(
    private _localization: LocalizationService,
  ) {
  }

  ngOnInit(): void {
    if (!this.form) {
      this.form = new FormGroup({
        [this.field.key]: this.pseudoFormControl,
      });
      this.pseudoFormControl.setValue(this.field.value);
    }

    this.setLabels();
    if (this.field.dropdownOptions) {
      const {items, label} = this.field.dropdownOptions;
      this.dropdownFilteredItems = items;
      this.dropdownSelectedItem.label = label;

      if (this.field.dropdownOptions.type === 'single' && 'selectedItemResolver' in this.field.dropdownOptions) {
        const item = items.find(item => (this.field.dropdownOptions as DropdownSingleOptions).selectedItemResolver!(item.key));
        if (item) {
          this.setSelectedItem(item, true);
        }
      } else if (this.field.dropdownOptions.type === 'multi') {
        this.multiSelect = true;
        this.control?.setValue([] as any);
        if ('selectedItemsResolver' in this.field.dropdownOptions) {
          const selectedItems = items.filter(item => (this.field.dropdownOptions as DropdownMultiOptions).selectedItemsResolver!(item.key));
          if (selectedItems) {
            this.control?.setValue(selectedItems.map(x => x.key));
            this.dropdownSelectedItems = selectedItems.map(x => ({key: x.key, label: x.label}));
          }
        }
      } else {
        this.control?.patchValue(-1);
      }

      this.setMultiSelectLabel();
    }

    this.control?.valueChanges?.subscribe(value => {
      if (this.field.type === 'dropdown') {
        const selectItem = this.dropdownFilteredItems.find(item => item.key === value);
        if (selectItem) {
          this.onDropdownSelect(selectItem, false);
        }
      }

      if (this.field.type === 'text' || this.field.type === 'textarea') {
        this.onChange.emit(value);
      }
    });

    if (this.field.type === 'dropdown') {
      if (this.control.value === undefined) {
        this.control?.patchValue(-1);
      }
    }
  }

  get control() {
    return this.form?.controls[this.field.key] ?? this.pseudoFormControl;
  }

  get dropdownSelectedItemClassName() {
    if (!this.field.dropdownOptions) {
      return {};
    }

    const className = {
      [dropdownSelectedItemVariants[this.field.dropdownOptions?.variant]]: true
    };

    if (this.field.dropdownOptions?.height) {
      className[dropdownHeightVariants[this.field.dropdownOptions?.height]] = true;
    } else {
      className[dropdownHeightVariants['base']] = true;
    }

    return className;
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
      this.multiSelectLabel = this._localization.translate('UI.LABEL.SELECTED', {count: this.dropdownSelectedItems.length.toString()}, false);
      return;
    }

    if (this.dropdownSelectedItem) {
      this.multiSelectLabel = this.dropdownSelectedItem.label;
      return;
    }

    this.multiSelectLabel = this.field.dropdownOptions.label;
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

    if (selectedItem.key !== -1) {
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
