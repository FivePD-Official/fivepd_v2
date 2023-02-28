import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Inject, OnDestroy,
  OnInit,
  Optional,
  Output,
  ViewChild
} from '@angular/core';
import { FormInputBase, FormSubmitEvent, MappedLooseDataEntity, ModalRef } from '@models';
import { ENTITY } from '../modal-entity-view/entity-token';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
})
export class ModalFormComponent implements OnInit, AfterViewInit, OnDestroy {

  destroy$ = new Subject<void>();
  fields: FormInputBase<any>[] = [];
  @ViewChild(DynamicFormComponent) dynamicFormComponent?: DynamicFormComponent;
  @Output() form = new EventEmitter();

  constructor(
    public readonly modalRef: ModalRef,
    private readonly looseDataService: LooseDataService,
    @Optional() @Inject(ENTITY) private mappedEntity: MappedLooseDataEntity,
  ) {
  }

  ngOnInit(): void {
    this.populateForm();
  }

  ngAfterViewInit(): void {
    this.form.emit(this.dynamicFormComponent);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
  }

  populateForm(): void {
    const fields = this.modalRef.options.fields;
    if (!fields) {
      return;
    }

    const valueResolver = this.modalRef.options.valueResolver;
    if (valueResolver) {
      valueResolver.value
        .pipe(takeUntil(this.destroy$))
        .subscribe(v => {
          this.fields = fields(v);
        });
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
