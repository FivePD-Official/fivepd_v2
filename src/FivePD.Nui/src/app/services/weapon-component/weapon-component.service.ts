import { Injectable } from '@angular/core';
import {Collation, Id, LoadoutConfig, SimpleUser, SimpleWeapon, SimpleWeaponComponent, WeaponComponent} from '@models';
import { CrudApiService } from "@services/api/crud-api/crud-api.service";
import {Observable} from "rxjs";
import { HttpClient } from "@angular/common/http";
import {Store} from "@ngrx/store";
import {GlobalState} from "@store";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";

@Injectable({
  providedIn: 'root'
})
export class WeaponComponentService extends CrudApiService<WeaponComponent> {

  override baseUrl = 'fivepd:ui:weaponComponent';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }

  getAllSimple(): Observable<SimpleWeaponComponent[]> {
    return this.httpClient.post<SimpleWeaponComponent[]>(this.createUrl('getAllSimple'), {});
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
