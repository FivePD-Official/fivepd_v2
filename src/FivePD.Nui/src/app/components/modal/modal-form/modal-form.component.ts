import { AfterViewInit, Component, EventEmitter, Inject, OnInit, Optional, Output, ViewChild } from '@angular/core';
import { FormInputBase, FormSubmitEvent, MappedLooseDataEntity, ModalRef } from '@models';
import { ENTITY } from '../modal-entity-view/entity-token';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { tap } from 'rxjs';

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
  styleUrls: ['./modal-form.component.sass']
})
export class ModalFormComponent implements OnInit, AfterViewInit {

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

  populateForm(): void {
    const fields = this.modalRef.options.fields;
    if (!fields) {
      return;
    }

    this.fields = fields(this.mappedEntity);
    if (!this.mappedEntity) {
      return;
    }

    this.modalRef.startLoading('Updating...');
    this.looseDataService.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id)
      .pipe(
        tap(() => this.modalRef.stopLoading())
      )
      .subscribe(entityWrapper => {
        this.fields = fields(entityWrapper.entity);
      });
  }

  submit(event?: FormSubmitEvent): void {
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
