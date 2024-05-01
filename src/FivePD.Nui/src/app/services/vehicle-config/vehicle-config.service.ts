import { Injectable } from '@angular/core';
import { Collation, Id, VehicleConfig, VehicleConfigType } from '@models';
import { BaseApiService } from "@services/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class VehicleConfigService extends BaseApiService<VehicleConfig> {

  override baseUrl = 'fivepd:ui:vehicleConfig';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation<{ Type: VehicleConfigType }>) {
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
