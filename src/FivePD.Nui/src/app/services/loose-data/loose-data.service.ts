import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  ActionResponse,
  AddSetAttribute, AttributeDataType,
  Collation,
  EditSetAttribute,
  LooseDataEntity, LooseDataEntityPageWrapper, LooseDataEntityWrapper,
  Page,
  SetAttribute
} from '@models';
import { map, Observable, switchMap, tap } from 'rxjs';
import { SetId } from '@enums';

@Injectable({
  providedIn: 'root'
})
export class LooseDataService {

  private _sets: Record<string, SetAttribute[]> = {};

  constructor(
    private _http: HttpClient,
  ) { }

  getSetAttributes(setId: SetId): SetAttribute[] {
    return this._sets[setId];
  }

  getSetAttributesPage(setId: SetId, collation: Collation): Observable<Page<SetAttribute>> {
    return this._http.post<Page<SetAttribute>>('fivepd:looseData:getSetAttributesPageable', {
      SetId: setId,
      ...collation
    });
  }

  addSetAttribute(setId: SetId, attribute: AddSetAttribute): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:addSetAttribute', {
      SetId: setId,
      Attribute: attribute
    });
  }

  editSetAttribute(setId: SetId, attribute: EditSetAttribute): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:editSetAttribute', {
      SetId: setId,
      Attribute: attribute
    });
  }

  deleteSetAttribute(setId: SetId, attributeId: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:deleteSetAttribute', {
      SetId: setId,
      AttributeId: attributeId
    });
  }

  getEntity(setId: SetId, id: number): Observable<LooseDataEntityWrapper> {
    return this.updateSetAttributes(setId).pipe(
      switchMap(() => this._http.post<LooseDataEntity>('fivepd:looseData:getEntity', {
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

  getEntities(setId: SetId, collation: Collation): Observable<LooseDataEntityPageWrapper> {
    return this.updateSetAttributes(setId).pipe(
      switchMap(() => this._http.post<Page<LooseDataEntity>>('fivepd:looseData:getEntities', {
        SetId: setId,
        ...collation,
      }).pipe(
        map(page => {
          page.Items = page.Items.map(entity => this.mapEntity(setId, entity))
          return {
            page: page,
            attributes: this.getSetAttributes(setId),
          };
        }),
      )),
    );
  }

  addEntity(setId: SetId, entity: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:addEntity', {
      SetId: setId,
      Entity: entity,
    });
  }

  editEntity(setId: SetId, entity: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:editEntity', {
      SetId: setId,
      Entity: entity,
    });
  }

  deleteEntity(setId: SetId, id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:looseData:deleteEntity', {
      SetId: setId,
      Id: id,
    });
  }

  private updateSetAttributes(setId: SetId): Observable<SetAttribute[]> {
    return this._http.post<SetAttribute[]>('fivepd:looseData:getSetAttributes', {
      SetId: setId,
    }).pipe(
      tap(attributes => this._sets[setId] = attributes)
    );
  }

  private mapEntity(setId: SetId, entity: LooseDataEntity): LooseDataEntity {
    const set = this.getSetAttributes(setId);
    for (const [key, value] of Object.entries(entity)) {
      const attr = set.find(s => s.Name === key);
      if (!attr) {
        continue;
      }

      if (attr.DataType === AttributeDataType.DateTime) {
        // @ts-ignore
        entity[key] = new Date(value);
      }
    }

    return entity;
  }

}
