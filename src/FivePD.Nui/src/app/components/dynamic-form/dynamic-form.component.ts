import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  QueryList,
  ViewChildren
} from '@angular/core';
import { FormInputBase, FormSubmitEvent } from '@models';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import deepEqual from 'fast-deep-equal/es6';
import { CustomValidators } from '../../helpers/validators';
import { DynamicFormInputComponent } from "../dynamic-form-input/dynamic-form-input.component";
import { ModalRef } from "@services/modals/modal/modal-ref";
import { Subject, takeUntil } from "rxjs";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ButtonComponent } from '../button/button.component';
import { NgClass, NgFor, NgIf, NgStyle } from '@angular/common';

type RemoveFocusTrap = () => void;

@Component({
  selector: 'app-dynamic-form',
  templateUrl: './dynamic-form.component.html',
  styleUrls: ['./dynamic-form.component.sass'],
  standalone: true,
  imports: [ReactiveFormsModule, NgClass, NgIf, NgStyle, NgFor, DynamicFormInputComponent, ButtonComponent, TranslatePipe]
})
export class DynamicFormComponent implements OnInit, OnChanges, OnDestroy, AfterViewInit {

  destroy$ = new Subject<void>();

  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() formFields: FormInputBase<any>[] = [];

  @Input() columns: number = 1;
  @Input() limitWidth: boolean = true;
  @Input() disableSubmitWithOriginalValue: boolean = false;
  @Input() buttonAlignment: 'left' | 'right' | 'full' = 'right';
  @Input() submitButtonLabel: string = '';

  @Input() showSubmitButton = false;

  @Input() modalRef?: ModalRef;
  @Input() focusFirstField = false;
  removeFocusTrap?: RemoveFocusTrap;
  modalRefLoading = false;
  currentFocus?: Element | null;

  @ViewChildren(DynamicFormInputComponent, {read: ElementRef}) inputs?: QueryList<ElementRef>;

  @Output() onSubmit = new EventEmitter<FormSubmitEvent>();

  form!: FormGroup;
  columnsGridStyle: string = '';
  formFieldColumns: FormInputBase<any>[][] = [];
  originalFormValue?: object;

  ngOnInit(): void {
    this.columnsGridStyle = `1fr / repeat(${this.columns}, 1fr)`;
    this.originalFormValue = this.form.value;
  }

  ngOnChanges(): void {
    this.form = this.createForm();
    this.createColumns();

    this.modalRef?.loading.pipe(takeUntil(this.destroy$)).subscribe(loading => this.modalRefLoading = loading);

    this.inputs?.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.removeFocusTrap = this.trapFocus();
    });
  }

  ngOnDestroy(): void {
    if (this.removeFocusTrap) {
      this.removeFocusTrap();
    }

    this.destroy$.next();
  }

  ngAfterViewInit(): void {
    if (!this.removeFocusTrap) {
      this.removeFocusTrap = this.trapFocus();
    }
  }

  createForm(): FormGroup {
    const group: Record<string, FormControl | FormGroup> = {};
    this.formFields.forEach(field => {
      let formControl;
      if (field.type === 'checkboxGroup') {
        const nestedGroup: Record<string, FormControl> = {};
        field.checkboxGroup.forEach(checkbox => {
          nestedGroup[checkbox.key] = new FormControl(checkbox.value || false, field.validators);
        });
        formControl = new FormGroup(nestedGroup);
      } else {
        const validators = [...field.validators];
        const asyncValidators = [...field.asyncValidators];

        if (field.required) {
          if (field.type === 'dropdown') {
            if (field.dropdownOptions?.type === 'single') {
              validators.push(CustomValidators.dropdownSingleRequired);
            } else {
              validators.push(CustomValidators.dropdownMultiRequired);
            }
          } else {
            validators.push(Validators.required);
          }
        }

        if (field.type === 'date') {
          validators.push(CustomValidators.date());
        }

        let fieldValue;
        if (field.type === 'number') {
          fieldValue = field.value === 0 ? 0 : field.value;
        } else if (!field.value && field.type === 'checkbox') {
          fieldValue = false;
        } else {
          fieldValue = field.value;
        }
        formControl = new FormControl(
          {value: fieldValue, disabled: field.readonly},
          validators,
          asyncValidators,
        );
      }
      group[field.key] = formControl;
    });
    const form = new FormGroup(group);

    if (this.disableSubmitWithOriginalValue) {
      form.valueChanges.subscribe(() => {
        const equal = deepEqual(this.originalFormValue, this.form.value);
        if (equal) {
          this.form.markAsPristine();
        } else {
          this.form.markAsDirty();
        }
      });
    }

    this.originalFormValue = form.value;

    return form;
  }

  createColumns(): void {
    const _formFields = [...this.formFields];
    const columns = [];
    for (let i = this.columns; i > 0; i--) {
      columns.push(_formFields.splice(0, Math.ceil(_formFields.length / i)));
    }
    this.formFieldColumns = columns;
  }

  internalOnSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.onSubmit.emit({
      value: this.form.value,
      markAsPristine: () => {
        this.originalFormValue = this.form.value;
        this.form.markAsPristine();
      }
    });
  }

  revert(): void {
    if (!this.originalFormValue) {
      return;
    }

    this.form.setValue(this.originalFormValue);
    this.form.markAsPristine();
  }

  private trapFocus(): RemoveFocusTrap {
    if (!this.focusFirstField) {
      return () => {
      };
    }

    const focusableEls = this.focusInFirstField();
    const firstFocusableEl = focusableEls[0];
    const lastFocusableEl = focusableEls[focusableEls.length - 1];
    this.currentFocus = focusableEls.length ? firstFocusableEl : null;

    const handleFocus = (e: FocusEvent) => {
      if (!e?.target) {
        return;
      }
      e.preventDefault();

      if (focusableEls.includes(e.target as HTMLElement)) {
        this.currentFocus = e.target as HTMLElement;
      } else {
        if (this.currentFocus === firstFocusableEl) {
          (lastFocusableEl as any)?.focus();
        } else {
          (firstFocusableEl as any)?.focus();
        }
        this.currentFocus = document.activeElement;
      }
    };

    document.addEventListener('focus', handleFocus, true);

    return () => {
      document.removeEventListener('focus', handleFocus, true);
    };
  }

  private focusInFirstField(): Element[] {
    if (!this.focusFirstField || !this.inputs || this.modalRefLoading) {
      return [];
    }

    const modal = document.querySelector('app-modal-form');
    if (!modal) {
      return [];
    }

    const focusableEls = [...this.inputs].reduce((prev, curr) => {
      const inputs = curr.nativeElement.querySelectorAll('input, textarea')
      for (const input of inputs) {
        prev.push(input);
      }
      return prev;
    }, [] as Element[]);

    if (!focusableEls.length) {
      return [];
    }

    const firstFocusableEl = focusableEls[0];
    if (firstFocusableEl.nodeName !== 'BUTTON') {
      this.currentFocus = firstFocusableEl;
      (firstFocusableEl as HTMLInputElement).focus();
    }

    return focusableEls;
  }
}
