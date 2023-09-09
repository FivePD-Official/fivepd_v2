import { Component, Inject, OnInit } from '@angular/core';
import { ATTRIBUTES, ENTITY } from './entity-token';
import {
  AttributeDataType, Department,
  IncidentHistory,
  LooseDataEntityWrapper,
  MappedLooseDataEntity,
  PedHistory,
  SetAttribute, User
} from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { formatDate } from '@helpers';
import { tap } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { Router } from "@angular/router";

interface EntityValueLink {
  label: string;
  link: (string | number)[];
}

interface EntityValue {
  name: string;
  value: string | EntityValueLink[];
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
    private readonly _router: Router,
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

  typeof(value: any): string {
    return typeof value;
  }

  onLinkClick(link: EntityValueLink): void {
    this.modalRef.close({value: {}, success: true});
    this._router.navigate(link.link);
  }

  private getEntity(): void {
    this.modalRef.startLoading('Updating...');
    this._looseData.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id)
      .pipe(
        tap(() => this.modalRef.stopLoading())
      )
      .subscribe(res => {
        console.log(res)
        this.mapEntityValues(res);
      });
  }

  private mapEntityValues(entityWrapper: LooseDataEntityWrapper): void {
    const values: EntityValue[] = [];
    for (const attr of entityWrapper.attributes) {
      let name = attr.Label ?? '<Missing label>';
      let value = entityWrapper.entity[attr.Name];

      if (value instanceof Date) {
        values.push({
          name,
          value: formatDate(value, 'none'),
        });
        continue;
      }

      let entityValue: EntityValue = {
        name,
        value,
      };

      // Replace dedicated field IDs with their Name
      if (name === 'Department' && entityWrapper.entity['Department']) {
        entityValue.name = 'Department';
        entityValue.value = entityWrapper.entity['Department']['Name'] ?? '-';
      }

      if (name === 'DepartmentIds' && entityWrapper.entity['Departments']) {
        entityValue.name = 'Departments';
        entityValue.value = entityWrapper.entity['DepartmentIds'].map((id: number) => {
          const department = (entityWrapper.entity['Departments'] as Department[]).find(x => x.Id === id);
          return {
            label: department ? department.Name : 'Unknown',
            link: ['mdt', 'departments', id],
          } satisfies EntityValueLink;
        });
      }

      if (name === 'User' && entityWrapper.entity['User']) {
        entityValue.name = 'User';
        entityValue.value = entityWrapper.entity['User']['Name'] ?? '-';
      }

      if (name === 'UserIds' && entityWrapper.entity['Users']) {
        entityValue.name = 'Users';
        entityValue.value = entityWrapper.entity['UserIds'].map((id: number) => {
          const user = (entityWrapper.entity['Users'] as User[]).find(x => x.Id === id);
          return {
            label: user ? user.Name : 'Unknown',
            link: ['mdt', 'users', id], // TODO: no route for this
          } satisfies EntityValueLink;
        });
      }

      if (name === 'IncidentHistory' && entityWrapper.entity['IncidentHistory']) {
        entityValue.name = 'Incident';
        entityValue.value = entityWrapper.entity['IncidentHistory']['Title'] ?? '-';
      }

      if (name === 'IncidentHistoryIds' && entityWrapper.entity['IncidentHistories']) {
        entityValue.name = 'Incidents';
        entityValue.value = entityWrapper.entity['IncidentHistoryIds'].map((id: string) => {
          const incidentHistory = (entityWrapper.entity['IncidentHistories'] as IncidentHistory[]).find(x => x.Id === id);
          return {
            label: incidentHistory ? incidentHistory.Title : 'Unknown',
            link: ['mdt', 'incidents', id],
          } satisfies EntityValueLink;
        });
      }

      if (name === 'PedHistory' && entityWrapper.entity['PedHistory']) {
        entityValue.name = 'Citizen'; // TODO: Rename?
        entityValue.value = entityWrapper.entity['PedHistory']['Title'] ?? '-';
      }

      if (name === 'PedHistoryIds' && entityWrapper.entity['PedHistories']) {
        entityValue.name = 'Citizens'; // TODO: Rename?
        entityValue.value = entityWrapper.entity['PedHistoryIds'].map((id: number) => {
          const pedHistory = (entityWrapper.entity['PedHistories'] as PedHistory[]).find(x => x.Id === id);
          return {
            label: pedHistory ? pedHistory.Firstname + ' ' + pedHistory.Lastname : 'Unknown',
            link: ['mdt', 'citizen-database', id],
          } satisfies EntityValueLink;
        });
      }

      // Replace enum index with value
      if (attr.Options?.Items) {
        entityValue.value = attr.Options.Items[value];
      }

      // In case of strings replace empty value with a dash
      if (
        (attr.DataType === AttributeDataType.ShortString ||
          attr.DataType === AttributeDataType.LongString) &&
        !value
      ) {
        entityValue.value = '-';
      }

      values.push(entityValue);
    }

    const columns = [];
    for (let i = this.modalRef.options?.columns ?? 1; i > 0; i--) {
      columns.push(values.splice(0, Math.ceil(values.length / i)));
    }
    this.entityValueColumns = columns;
  }

}
