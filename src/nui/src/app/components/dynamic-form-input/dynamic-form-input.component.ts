import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { DropdownItem, FormInputBase } from '@models';
import getKeyFromObjectPath from 'lodash-es/get';
import { LocalizationService } from '@services/localization/localization.service';

@Component({
  selector: 'app-dynamic-form-input',
  templateUrl: './dynamic-form-input.component.html',
  styleUrls: ['./dynamic-form-input.component.sass']
})
export class DynamicFormInputComponent implements OnInit {

  @Input() form!: FormGroup;
  @Input() field!: FormInputBase<string>;

  label: string = '';
  helpLabel: string = '';

  dropdownOpen: boolean = false;
  dropdownSelectedItem: { key: number, label: string } = { key: -1, label: '' };
  dropdownFilteredItems: DropdownItem[] = [];

  constructor(
    private _localization: LocalizationService,
  ) { }

  ngOnInit(): void {
    this.setLabels();
    if (this.field.dropdownOptions?.items) {
      this.dropdownFilteredItems = this.field.dropdownOptions.items;
      this.dropdownSelectedItem.label = this.field.dropdownOptions.label;
      if (this.field.dropdownOptions.selectedItem) {
        const item = this.field.dropdownOptions.items.find(x => x.key === this.field.dropdownOptions?.selectedItem);
        if (item) {
          this.setSelectedItem(item);
        }
      } else {
        this.field.dropdownOptions.selectedItem = -1;
      }
      this.form.controls[this.field.key].patchValue(this.field.dropdownOptions.selectedItem);
    }
  }

  get control() {
    return this.form.controls[this.field.key];
  }

  onDropdownSelect(selectItem: DropdownItem): void {
    this.dropdownOpen = false;
    this.setSelectedItem(selectItem);
  }

  private setSelectedItem(selectedItem: DropdownItem) {
    this.form.controls[this.field.key].patchValue(selectedItem.key);
    this.dropdownSelectedItem = selectedItem;
    this.dropdownFilteredItems = this.field.dropdownOptions!.items!.filter(item => item.key !== selectedItem.key);

    if (selectedItem.key !== this.field.dropdownOptions?.selectedItem) {
      this.dropdownFilteredItems.unshift({ key: -1, label: this.field.dropdownOptions!.label });
    }
  }

  private setLabels(): void {
    this.field.label = this._localization.translate(this.field.label);
    this.label = this.field.label;
    if (this.field.required) {
      this.label = `${this.field.label}*`;
    }

    this.helpLabel = this._localization.translate(this.field.helpLabel);
  }

}
