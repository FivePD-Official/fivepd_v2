import { AfterViewInit, Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { FormInputBase, FormSubmitEvent, ModalFormOptions } from '@models';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { Subject } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
})
export class ModalFormComponent implements OnInit, AfterViewInit {

  destroy$ = new Subject<void>();
  fields: FormInputBase<any>[] = [];
  @ViewChild(DynamicFormComponent) dynamicFormComponent?: DynamicFormComponent;
  @Output() form = new EventEmitter();

  constructor(
    public readonly modalRef: ModalRef,
  ) {
  }

  ngOnInit(): void {
    this.populateForm();
  }

  ngAfterViewInit(): void {
    this.form.emit(this.dynamicFormComponent);
  }

  populateForm(): void {
    const { fields, valueResolver } = this.modalRef.options as ModalFormOptions<any>;
    if (!fields) {
      return;
    }

    if (valueResolver) {
      valueResolver.value
        .pipe(takeUntilDestroyed())
        .subscribe(v => {
          this.fields = fields(v);
        });
    } else {
      this.fields = fields(undefined);
    }
  }

  submit(event?: FormSubmitEvent): void {
    this.modalRef.startLoading('Loading...');
    this.modalRef.close({
      success: true,
      value: event?.value ?? {},
    });
  }

  close(): void {
    this.modalRef.close({
      success: false,
      value: {}
    });
  }

}
