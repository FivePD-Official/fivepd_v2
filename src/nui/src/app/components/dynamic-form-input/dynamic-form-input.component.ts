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
          this.setSelectedItem(item);
        }
      } else {
        this.form?.controls[this.field.key].patchValue(-1);
      }
    }

    this.form?.controls[this.field.key].valueChanges.subscribe(value => {
      this.onChange.emit(value);
    });
  }

  get control() {
    return this.form?.controls[this.field.key] ?? this.pseudoFormControl;
  }

  onDropdownSelect(selectItem: DropdownItem): void {
    this.dropdownOpen = false;
    this.setSelectedItem(selectItem);
  }

  private setSelectedItem(selectedItem: DropdownItem) {
    if (!this.field.dropdownOptions) {
      return;
    }

    const { items, label } = this.field.dropdownOptions;

    this.form?.controls[this.field.key].patchValue(selectedItem.key);
    this.dropdownSelectedItem = selectedItem;
    this.dropdownFilteredItems = items.filter(item => item.key !== selectedItem.key);

    if (selectedItem.key !== -1) {
      this.dropdownFilteredItems.unshift({ key: -1, label: label });
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
