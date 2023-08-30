import { Component, Inject, OnInit } from '@angular/core';
import { ATTRIBUTES, ENTITY } from './entity-token';
import { AttributeDataType, LooseDataEntityWrapper, MappedLooseDataEntity, SetAttribute } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { formatDate } from '@helpers';
import { tap } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';

interface EntityValue {
  name: string;
  value: string;
}

@Component({
  selector: 'app-modal-entity-view',
  templateUrl: './modal-entity-view.component.html',
  styleUrls: ['./modal-entity-view.component.sass']
})
export class ModalEntityViewComponent implements OnInit {

  entityValueColumns: EntityValue[][] = [];
  columnsGridStyle: string = '';

  constructor(
    private modalRef: ModalRef,
    @Inject(ENTITY) private mappedEntity: MappedLooseDataEntity,
    @Inject(ATTRIBUTES) private attributes: SetAttribute[],
    private _looseData: LooseDataService,
  ) {
  }

  ngOnInit(): void {
    this.mapEntityValues({
      entity: this.mappedEntity,
      attributes: this.attributes
    });
    this.getEntity();
    this.columnsGridStyle = `1fr / repeat(${this.modalRef.options?.columns ?? 1}, 1fr)`;
  }

  private getEntity(): void {
    this.modalRef.startLoading('Updating...');
    this._looseData.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id)
      .pipe(
        tap(() => this.modalRef.stopLoading())
      )
      .subscribe(res => {
        this.mapEntityValues(res);
      });
  }

  private mapEntityValues(entityWrapper: LooseDataEntityWrapper): void {
    const values: EntityValue[] = [];
    for (const attr of entityWrapper.attributes) {
      let value = entityWrapper.entity[attr.Name];
      if (value instanceof Date) {
        value = formatDate(value);
      }

      let name = attr.Label ?? '<Missing label>';
      // Replace dedicated field IDs with their Name
      if (name === 'Department' && entityWrapper.entity['Department']) {
        name = 'Department';
        value = entityWrapper.entity['Department']['Name'] ?? '-';
      }
      if (name === 'User' && entityWrapper.entity['User']) {
        name = 'User';
        value = entityWrapper.entity['User']['Name'] ?? '-';
      }
      if (name === 'Incident' && entityWrapper.entity['Incident']) {
        name = 'Incident';
        value = entityWrapper.entity['Incident']['Title'] ?? '-';
      }

      // Replace enum index with value
      if (attr.Options?.Items) {
        value = attr.Options.Items[value];
      }

      // In case of strings replace empty value with a dash
      if (
        (attr.DataType === AttributeDataType.ShortString ||
          attr.DataType === AttributeDataType.LongString) &&
        !value
      ) {
        value = '-';
      }

      values.push({
        name: name,
        value,
      });
    }

    const columns = [];
    for (let i = this.modalRef.options?.columns ?? 1; i > 0; i--) {
      columns.push(values.splice(0, Math.ceil(values.length / i)));
    }
    this.entityValueColumns = columns;
  }

}
