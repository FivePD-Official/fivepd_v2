import {Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  ActionResponse,
  AddSetAttribute,
  AttributeDataType,
  Collation,
  EditSetAttribute,
  VariableDataEntity,
  VariableDataEntityPageWrapper,
  VariableDataEntityWrapper,
  MappedVariableDataEntity,
  Page,
  VariableDataAttribute, Department, User, IncidentHistory, PedData
} from '@models';
import {map, Observable, switchMap, tap} from 'rxjs';
import {SetId} from '@enums';
import {formatDate} from "@helpers";

export interface EntityValueLink {
  label: string;
  link: (string | number)[];
}

export interface EntityValue {
  name: string;
  value: string | EntityValueLink[];
}

@Injectable({
  providedIn: 'root'
})
export class VariableDataService {

  private _sets: Record<string, VariableDataAttribute[]> = {};

  constructor(
    private _http: HttpClient,
  ) {
  }

  getSetAttribute(setId: SetId, attributeId: number): Observable<VariableDataAttribute> {
    return this.updateSetAttribute(setId, attributeId);
  }

  getSetAttributes(setId: SetId): VariableDataAttribute[] {
    return this._sets[setId];
  }

  getSetAttributesPage(setId: SetId, collation: Collation): Observable<Page<VariableDataAttribute>> {
    return this._http.post<Page<VariableDataAttribute>>('fivepd:variableData:getSetAttributesPageable', {
      SetId: setId,
      ...collation
    });
  }

  addSetAttribute(setId: SetId, attributeType: string, attribute: AddSetAttribute): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:addSetAttribute', {
      SetId: setId,
      AttributeType: attributeType,
      Attribute: attribute
    });
  }

  editSetAttribute(setId: SetId, attribute: EditSetAttribute): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:editSetAttribute', {
      SetId: setId,
      Attribute: attribute
    });
  }

  deleteSetAttribute(setId: SetId, attributeId: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:deleteSetAttribute', {
      SetId: setId,
      AttributeId: attributeId
    });
  }

  getEntity(setId: SetId, id: number): Observable<VariableDataEntityWrapper> {
    return this.updateSetAttributes(setId).pipe(
      switchMap(() => this._http.post<VariableDataEntity>('fivepd:variableData:getEntity', {
        SetId: setId,
        Id: id,
      }).pipe(
        map(entity => {
          return {
            entity: this.mapEntity(setId, entity),
            attributes: this.getSetAttributes(setId),
          }
        }),
      )),
    );
  }

  getEntitiesPageable(setId: SetId, collation: Collation): Observable<VariableDataEntityPageWrapper> {
    return this.updateSetAttributes(setId).pipe(
      switchMap(() => this._http.post<Page<VariableDataEntity>>('fivepd:variableData:getEntitiesPageable', {
        SetId: setId,
        ...collation,
      }).pipe(
        map(page => {
          page.Items = page.Items.map(entity => this.mapEntity(setId, entity)) as VariableDataEntity[];
          return {
            page: page,
            attributes: this.getSetAttributes(setId),
          };
        }),
      )),
    );
  }

  getEntities(setId: SetId, filter: Record<string, any>): Observable<MappedVariableDataEntity[]> {
    return this._http.post<VariableDataEntity[]>('fivepd:variableData:getEntities', {
      SetId: setId,
      Filter: filter,
    }).pipe(
      map(entities => entities.map(entity => this.mapEntity(setId, entity))),
    );
  }

  addEntity(setId: SetId, entity: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:addEntity', {
      SetId: setId,
      Entity: entity,
    });
  }

  editEntity(setId: SetId, entityId: number, entity: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:editEntity', {
      SetId: setId,
      EntityId: entityId,
      Entity: entity,
    });
  }

  deleteEntity(setId: SetId, id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:variableData:deleteEntity', {
      SetId: setId,
      Id: id,
    });
  }

  updateSetAttributes(setId: SetId): Observable<VariableDataAttribute[]> {
    return this._http.post<VariableDataAttribute[]>('fivepd:variableData:getSetAttributes', {
      SetId: setId,
    }).pipe(
      tap(attributes => this._sets[setId] = attributes)
    );
  }

  mapEntityValues(entityWrapper: VariableDataEntityWrapper): EntityValue[] {
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

      if (attr.Name === 'DepartmentId' && entityWrapper.entity['Department']) {
        entityValue.value = entityWrapper.entity['Department']['Name'] ?? '-'; // TODO: make this a link
      }

      if (attr.Name === 'DepartmentIds' && entityWrapper.entity['Departments']) {
        entityValue.value = entityWrapper.entity['DepartmentIds'].map((id: number) => {
          const department = (entityWrapper.entity['Departments'] as Department[]).find(x => x.Id === id);
          return {
            label: department ? department.Name : 'Unknown',
            link: ['mdt', 'departments', id],
          } satisfies EntityValueLink;
        });
      }

      if (attr.Name === 'UserId' && entityWrapper.entity['User']) {
        entityValue.value = entityWrapper.entity['User']['Name'] ?? '-';
      }

      if (attr.Name === 'UserIds' && entityWrapper.entity['Users']) {
        entityValue.value = entityWrapper.entity['UserIds'].map((id: number) => {
          const user = (entityWrapper.entity['Users'] as User[]).find(x => x.Id === id);
          return {
            label: user ? user.Name : 'Unknown',
            link: ['mdt', 'users', id], // TODO: no route for this
          } satisfies EntityValueLink;
        });
      }

      if (attr.Name === 'IncidentHistoryId' && entityWrapper.entity['IncidentHistory']) {
        entityValue.value = entityWrapper.entity['IncidentHistory']['Title'] ?? '-';  // TODO: make this a link
      }

      if (attr.Name === 'IncidentHistoryIds' && entityWrapper.entity['IncidentHistories']) {
        entityValue.value = entityWrapper.entity['IncidentHistoryIds'].map((id: string) => {
          const incidentHistory = (entityWrapper.entity['IncidentHistories'] as IncidentHistory[]).find(x => x.Id === id);
          return {
            label: incidentHistory ? incidentHistory.Title : 'Unknown',
            link: ['mdt', 'incidents', id],
          } satisfies EntityValueLink;
        });
      }

      if (attr.Name === 'PedHistoryId' && entityWrapper.entity['PedHistory']) {
        entityValue.value = entityWrapper.entity['PedHistory']['Title'] ?? '-';  // TODO: make this a link
      }

      if (attr.Name === 'PedHistoryIds' && entityWrapper.entity['PedHistories']) {
        entityValue.value = entityWrapper.entity['PedHistoryIds'].map((id: number) => {
          const pedHistory = (entityWrapper.entity['PedHistories'] as PedData[]).find(x => x.Id === id);
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

    return values;
  }

  private updateSetAttribute(setId: SetId, attributeId: number): Observable<VariableDataAttribute> {
    return this._http.post<VariableDataAttribute>('fivepd:variableData:getSetAttribute', {
      Id: attributeId,
    }).pipe(
      tap(attribute => {
        if (!Object.entries(this._sets).length) {
          return;
        }

        const cachedAttributeIndex = this._sets[setId].findIndex(a => a.Id === attributeId);
        this._sets[setId][cachedAttributeIndex] = attribute;
      })
    );
  }

  private mapEntity(setId: SetId, entity: VariableDataEntity): MappedVariableDataEntity {
    const attributes = this.getSetAttributes(setId);
    const mappedEntity: MappedVariableDataEntity = {
      Id: entity.Id,
      SetId: entity.SetId,
      ...entity.MappedValues,
    };

    for (const [key, value] of Object.entries(entity.MappedValues)) {
      const attr = attributes.find(s => s.Name === key);
      if (!attr) {
        continue;
      }

      if (attr.DataType === AttributeDataType.Date) {
        mappedEntity[key] = new Date(value);
      }
    }

    return mappedEntity;
  }

}
