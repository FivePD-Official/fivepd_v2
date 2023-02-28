import {
  AfterViewInit,
  Component,
  EventEmitter,
  Inject,
  OnDestroy,
  OnInit,
  Optional,
  Output,
  ViewChild
} from '@angular/core';
import { FormInputBase, FormSubmitEvent, MappedLooseDataEntity, ModalFormOptions } from '@models';
import { ENTITY } from '../modal-entity-view/entity-token';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { Subject, takeUntil } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';

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
    const { fields, valueResolver } = this.modalRef.options as ModalFormOptions<any>;
    if (!fields) {
      return;
    }

    if (valueResolver) {
      valueResolver.value
        .pipe(takeUntil(this.destroy$))
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
