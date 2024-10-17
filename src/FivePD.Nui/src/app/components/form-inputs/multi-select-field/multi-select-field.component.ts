import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseFieldComponent } from "../base-field/base-field.component";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { ClickOutsideDirective } from "../../../directives/click-outside.directive";
import { DropdownItem, MultiSelectField } from "@models";
import {
  dropdownHeightVariants,
  dropdownSelectedItemVariants,
  dropdownSelectedItemVariantsHover,
  dropdownSelectedItemVariantsHoverReadonly
} from "../_helpers/dropdown";
import { LocalizationService } from "@services/localization/localization.service";
import { createDefaultFormGroup } from "../_helpers/field";
import { DropdownRectHelperDirective } from "../../../directives/dropdown-rect-helper.directive";

@Component({
  selector: 'app-multi-select-field',
  standalone: true,
  imports: [CommonModule, BaseFieldComponent, ReactiveFormsModule, TranslatePipe, ClickOutsideDirective, DropdownRectHelperDirective],
  templateUrl: './multi-select-field.component.html',
  styleUrl: './multi-select-field.component.sass'
})
export class MultiSelectFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: MultiSelectField;

  @Output() onChange = new EventEmitter<number[]>();

  private pseudoFormControl = new FormControl<number[]>([]);

  open = false;
  dropdownSelectedItemLabel = '';
  dropdownSelectedItems: DropdownItem[] = [];
  dropdownFilteredItems: DropdownItem[] = [];

  constructor(private readonly _localizationService: LocalizationService) {
  }

  ngOnInit(): void {
    if (!this.form) {
      this.form = createDefaultFormGroup(this.field, []);
    }

    if (!this.control) {
      throw new Error('Form control is undefined.');
    }

    this.setSelectedItems(undefined);

    const selectedItems = this.field.items.filter(item => (this.field.value ?? []).includes(item.key));
    for (const item of selectedItems) {
      this.setSelectedItems(item, true);
    }

    this.control.valueChanges.subscribe(v => {
      if (Array.isArray(this.control.value) && !this.control.value.length) {
        this.control?.patchValue([], {emitEvent: false});
      }

      this.onChange.emit(this.dropdownSelectedItems.map(x => x.key));
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

  onDropdownSelect(item: DropdownItem): void {
    this.open = false;
    this.setSelectedItems(item);
  }

  isItemSelected(item: DropdownItem) {
    return this.dropdownSelectedItems.some(x => x.key === item.key);
  }

  private setSelectedItems(item: DropdownItem | undefined, toggleItem = true): void {
    if (item) {
      let value = [...this.control.value];
      if (toggleItem) {
        const valueIndex = value.indexOf(item.key);
        if (valueIndex > -1) {
          value.splice(valueIndex, 1);
        } else {
          value.push(item.key);
        }
      }

      const valueIndex2 = this.dropdownSelectedItems.findIndex(x => x.key === item.key);
      if (valueIndex2 > -1) {
        this.dropdownSelectedItems.splice(valueIndex2, 1);
      } else {
        this.dropdownSelectedItems.push(item);
      }

      this.control?.patchValue(value);

      this.dropdownFilteredItems = this.field.items.filter(item => !this.dropdownSelectedItems.includes(item.key));
      this.updateFormState();
    } else {
      this.dropdownFilteredItems = this.field.items;
      this.dropdownSelectedItemLabel = this.field.label;
    }

    if (this.dropdownSelectedItems.length) {
      this.dropdownSelectedItemLabel = this._localizationService.translate('UI.LABEL.SELECTED', {count: this.dropdownSelectedItems.length.toString()}, false);
    } else {
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
