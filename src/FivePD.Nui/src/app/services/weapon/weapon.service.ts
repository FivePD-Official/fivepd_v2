import { Injectable } from '@angular/core';
import {Collation, Id, LoadoutConfig, SimpleWeapon, SimpleWeaponComponent, Weapon} from '@models';
import { CrudApiService } from "@services/api/crud-api/crud-api.service";
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class WeaponService extends CrudApiService<Weapon> {

  override baseUrl = 'fivepd:ui:weapon';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }

  getAllSimple(): Observable<SimpleWeapon[]> {
    return this.httpClient.post<SimpleWeapon[]>('fivepd:ui:weapon:getAllSimple', {});
  }

  create(request: object) {
    return this._create('create', request);
  }

  edit(request: object) {
    return this._edit('edit', request);
  }

  delete(id: Id) {
    return this._delete('delete', id);
  }
}
