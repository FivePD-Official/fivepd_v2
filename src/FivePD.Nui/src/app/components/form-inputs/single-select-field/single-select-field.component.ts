import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseFieldComponent } from "../base-field/base-field.component";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { DropdownItem, SingleSelectField } from "@models";
import { ClickOutsideDirective } from "../../../directives/click-outside.directive";
import {
  dropdownHeightVariants,
  dropdownSelectedItemVariants,
  dropdownSelectedItemVariantsHover,
  dropdownSelectedItemVariantsHoverReadonly
} from "../_helpers/dropdown";
import { createDefaultFormGroup } from "../_helpers/field";
import { DropdownRectHelperDirective } from "../../../directives/dropdown-rect-helper.directive";

@Component({
  selector: 'app-single-select-field',
  standalone: true,
  imports: [CommonModule, BaseFieldComponent, ReactiveFormsModule, TranslatePipe, ClickOutsideDirective, DropdownRectHelperDirective],
  templateUrl: './single-select-field.component.html',
  styleUrl: './single-select-field.component.sass'
})
export class SingleSelectFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: SingleSelectField;

  @Output() onChange = new EventEmitter<number>();

  private pseudoFormControl = new FormControl<number | null>(-1);

  open = false;
  dropdownSelectedItemLabel = '';
  dropdownSelectedItem: DropdownItem = {key: -1, label: ''};
  dropdownFilteredItems: DropdownItem[] = [];

  ngOnInit(): void {
    if (!this.form) {
      this.form = createDefaultFormGroup(this.field, -1);
    }

    if (!this.control) {
      throw new Error('Form control is undefined.');
    }

    const selectedItem = this.field.items.find(item => item.key === this.field.value);
    this.setSelectedItem(selectedItem);

    this.control.valueChanges.subscribe(v => {
      if (this.control.value === undefined && this.field.value === undefined) {
        this.control?.patchValue(-1);
      }

      this.onChange.emit(v);
    });
  }

  get control() {
    return (this.form?.controls[this.field.key] ?? this.pseudoFormControl) as FormControl;
  }

  get dropdownSelectedItemClassName() {
    const className = {
      [dropdownSelectedItemVariants[this.field.variant]]: true,
      [dropdownSelectedItemVariantsHover[this.field.variant]]: !this.field.readonly,
      [dropdownSelectedItemVariantsHoverReadonly[this.field.variant]]: this.field.readonly,
    };

    if (this.field.height) {
      className[dropdownHeightVariants[this.field.height]] = true;
    } else {
      className[dropdownHeightVariants['base']] = true;
    }

    return className;
  }

  toggleDropdown(): void {
    if (this.field.readonly) {
      return;
    }

    this.open = !this.open;
  }

  onDropdownSelect(selectItem: DropdownItem): void {
    this.open = false;
    this.setSelectedItem(selectItem);
  }

  private setSelectedItem(selectedItem: DropdownItem | undefined): void {
    if (selectedItem) {
      this.dropdownSelectedItem = selectedItem;
      this.control?.patchValue(selectedItem.key);

      this.dropdownFilteredItems = this.field.items.filter(item => item.key !== selectedItem.key);

      const addDefaultItem = this.field.addDefaultItem === undefined || this.field.addDefaultItem;
      if (addDefaultItem && selectedItem.key !== -1) {
        this.dropdownFilteredItems.unshift({key: -1, label: this.field.defaultItemLabel || this.field.label});
      }

      this.updateFormState();
      this.dropdownSelectedItemLabel = this.dropdownSelectedItem.label;
    } else {
      this.dropdownFilteredItems = this.field.items;
      this.dropdownSelectedItemLabel = this.field.defaultItemLabel || this.field.label;
    }
  }

  private updateFormState(): void {
    if (this.control.value === this.field.value) {
      this.control.markAsPristine();
    } else {
      this.control.markAsDirty();
    }
  }
}
