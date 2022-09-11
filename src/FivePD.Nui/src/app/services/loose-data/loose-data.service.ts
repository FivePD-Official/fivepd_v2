import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LooseDataEntity, Set, SetAttribute } from '@models';
import { Observable } from 'rxjs';
import { SetId } from '@enums';

@Injectable({
  providedIn: 'root'
})
export class LooseDataService {

  private _sets: Record<string, SetAttribute[]> = {};

  constructor(
    private _http: HttpClient,
  ) { }

  loadSets(): void {
    this._http.post<Set[]>('fivepd:looseData:getSets', {}).subscribe(res => {
      this._sets = res.reduce((prev: Record<string, SetAttribute[]>, curr: Set) => {
        prev[curr.Id] = curr.Attributes;
        return prev;
      }, {});
    });
  }

  getAttributes(setId: SetId): SetAttribute[] {
    return this._sets[setId];
  }

  getEntity<T extends LooseDataEntity>(setId: SetId, id: number): Observable<T> {
    return this._http.post<T>('fivepd:looseData:getEntity', {
      SetId: setId,
      Id: id,
    });
  }

  getEntities<T extends LooseDataEntity>(setId: SetId): Observable<T[]> {
    return this._http.post<T[]>('fivepd:looseData:getEntities', {
      SetId: setId
    });
  }

}
