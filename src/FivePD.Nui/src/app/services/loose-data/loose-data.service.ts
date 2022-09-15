import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  ActionResponse,
  AddSetAttribute,
  AttributeDataType,
  Collation, EditSetAttribute,
  LooseDataEntity,
  Page,
  Set,
  SetAttribute
} from '@models';
import { map, Observable } from 'rxjs';
import { SetId } from '@enums';

@Injectable({
  providedIn: 'root'
})
export class LooseDataService {

  private _sets: Record<string, SetAttribute[]> = {};

  constructor(
    private _http: HttpClient,
  ) { }

  getSets(): void {
    this._http.post<Set[]>('fivepd:looseData:getSets', {}).subscribe(res => {
      this._sets = res.reduce((prev: Record<string, SetAttribute[]>, curr: Set) => {
        prev[curr.Id] = curr.Attributes;
        return prev;
      }, {});
    });
  }

  getSetAttributesPage(setId: SetId, collation: Collation): Observable<Page<SetAttribute>> {
    return this._http.post<Page<SetAttribute>>('fivepd:looseData:getSetAttributes', {
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

  getSetAttributes(setId: SetId): SetAttribute[] {
    return this._sets[setId];
  }

  getEntity<T extends LooseDataEntity>(setId: SetId, id: number): Observable<T> {
    return this._http.post<T>('fivepd:looseData:getEntity', {
      SetId: setId,
      Id: id,
    }).pipe(
      map(entity => this.mapEntity(setId, entity))
    );
  }

  getEntities<T extends LooseDataEntity>(setId: SetId): Observable<T[]> {
    return this._http.post<T[]>('fivepd:looseData:getEntities', {
      SetId: setId
    }).pipe(
      map(entities => entities.map(entity => this.mapEntity(setId, entity)))
    );
  }

  private mapEntity<T extends LooseDataEntity>(setId: SetId, entity: T): T {
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
