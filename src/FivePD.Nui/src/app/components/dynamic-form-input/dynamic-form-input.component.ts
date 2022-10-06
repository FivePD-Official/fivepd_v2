import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { DropdownItem, FormInputBase } from '@models';
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

  label: string = '';
  placeholder: string = '';
  helpLabel: string = '';
  pseudoFormControl = new FormControl('');

  dropdownOpen: boolean = false;
  dropdownSelectedItem: { key: number, label: string } = { key: -1, label: '' };
  dropdownFilteredItems: DropdownItem[] = [];

  constructor(
    private _localization: LocalizationService,
  ) { }

  ngOnInit(): void {
    if (!this.form) {
      this.form = new FormGroup({
        [this.field.key]: this.pseudoFormControl,
      });
    }

    this.setLabels();
    if (this.field.dropdownOptions) {
      const { items, selectedItemResolver, label } = this.field.dropdownOptions;
      this.dropdownFilteredItems = items;
      this.dropdownSelectedItem.label = label;

      if (selectedItemResolver) {
        const item = items.find(item => selectedItemResolver(item.key));
        if (item) {
          this.setSelectedItem(item, true);
        }
      } else {
        this.control?.patchValue(-1);
      }
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
    this.setSelectedItem(selectItem);
  }

  private setSelectedItem(selectedItem: DropdownItem, isCalledAtInit: boolean = false): void {
    if (!this.field.dropdownOptions) {
      return;
    }

    this.form?.controls[this.field.key].patchValue(selectedItem.key);
    this.dropdownSelectedItem = selectedItem;
    this.filterDropdownItems();

    if (selectedItem.key !== -1) {
      this.dropdownFilteredItems.unshift({ key: -1, label: this.field.dropdownOptions.label });
    }

    if (!isCalledAtInit) {
      const control = this.form?.controls[this.field.key];
      if (control?.value === this.field.value) {
        control?.markAsPristine();
      } else {
        control?.markAsDirty();
      }
    }
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
