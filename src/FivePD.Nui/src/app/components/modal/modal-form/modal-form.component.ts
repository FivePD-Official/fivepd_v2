import { AfterViewInit, Component, Inject, OnInit, Optional } from '@angular/core';
import { FormInputBase, FormSubmitEvent, MappedLooseDataEntity, ModalRef } from '@models';
import { ENTITY } from '../modal-entity-view/entity-token';
import { LooseDataService } from '@services/loose-data/loose-data.service';

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
  styleUrls: ['./modal-form.component.sass']
})
export class ModalFormComponent implements OnInit {

  fields: FormInputBase<any>[] = [];

  constructor(
    public readonly modalRef: ModalRef,
    private readonly looseDataService: LooseDataService,
    @Optional() @Inject(ENTITY) private mappedEntity: MappedLooseDataEntity,
  ) { }

  ngOnInit(): void {
    const fields = this.modalRef.options.fields;
    if (fields) {
      console.log(this.mappedEntity)
      this.fields = fields(this.mappedEntity);
      if (this.mappedEntity) {
        this.looseDataService.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id).subscribe(entityWrapper => {
          this.fields = fields(entityWrapper.entity);
        });
      }
    }
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
