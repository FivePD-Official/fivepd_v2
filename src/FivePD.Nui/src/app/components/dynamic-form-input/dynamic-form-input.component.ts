import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { DropdownItem, DropdownMultiOptions, DropdownSingleOptions, FormInputBase } from '@models';
import { LocalizationService } from '@services/localization/localization.service';

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
  dropdownSelectedItem: { key: number, label: string } = {key: -1, label: ''};
  dropdownSelectedItems: { key: number, label: string }[] = [];
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
    }

    this.setLabels();
    if (this.field.dropdownOptions) {
      const {items, label} = this.field.dropdownOptions;
      this.dropdownFilteredItems = items;
      this.dropdownSelectedItem.label = label;

      if ('selectedItemResolver' in this.field.dropdownOptions) {
        const item = items.find(item => (this.field.dropdownOptions as DropdownSingleOptions).selectedItemResolver!(item.key));
        if (item) {
          this.setSelectedItem(item, true);
        }
      } else if ('selectedItemsResolver' in this.field.dropdownOptions) {
        this.multiSelect = true;
        this.pseudoFormControl.setValue([] as any);
        const selectedItems = items.filter(item => (this.field.dropdownOptions as DropdownMultiOptions).selectedItemsResolver!(item.key));
        if (selectedItems) {
          this.form?.controls[this.field.key].setValue(selectedItems.map(x => x.key));
          this.dropdownSelectedItems = selectedItems.map(x => ({key: x.key, label: x.label}));
        }
      } else {
        this.control?.patchValue(-1);
      }

      this.setMultiSelectLabel();
    }

    this.control?.valueChanges?.subscribe(value => {
      this.onChange.emit(value);
    });
  }

  get control() {
    return this.form?.controls[this.field.key] ?? this.pseudoFormControl;
  }

  toggleDropdown(): void {
    if (this.field.readonly) {
      return;
    }

    this.dropdownOpen = !this.dropdownOpen;
  }

  onDropdownSelect(selectItem: DropdownItem): void {
    this.dropdownOpen = false;

    if ('selectedItemResolver' in this.field.dropdownOptions!) {
      this.setSelectedItem(selectItem);
    } else if ('selectedItemsResolver' in this.field.dropdownOptions!) {
      this.setSelectedItems(selectItem);
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

  private setSelectedItem(selectedItem: DropdownItem, isCalledAtInit: boolean = false): void {
    if (!this.field.dropdownOptions) {
      return;
    }

    this.form?.controls[this.field.key].patchValue(selectedItem.key);
    this.dropdownSelectedItem = selectedItem;
    this.filterDropdownItems();

    if (selectedItem.key !== -1) {
      this.dropdownFilteredItems.unshift({key: -1, label: this.field.dropdownOptions.label});
    }

    this.setMultiSelectLabel();
    this.updateFormState(isCalledAtInit);
  }

  private setSelectedItems(selectedItem: DropdownItem, isCalledAtInit: boolean = false): void {
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
    this.form?.controls[this.field.key].setValue(value);

    const valueIndex2 = this.dropdownSelectedItems.findIndex(x => x.key === selectedItem.key);
    if (valueIndex2 > -1) {
      this.dropdownSelectedItems.splice(valueIndex2, 1);
    } else {
      this.dropdownSelectedItems.push(selectedItem);
    }

    this.setMultiSelectLabel();
    this.filterDropdownItems();
    this.updateFormState(isCalledAtInit);
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
