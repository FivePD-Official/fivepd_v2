import {Injectable} from '@angular/core';
import {Collation, Id, LoadoutConfig} from '@models';
import {CrudApiService} from "@services/api/crud-api/crud-api.service";

@Injectable({
  providedIn: 'root'
})
export class LoadoutConfigService extends CrudApiService<LoadoutConfig> {

  override baseUrl = 'fivepd:ui:loadoutConfig';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
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
