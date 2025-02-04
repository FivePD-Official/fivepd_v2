import { Injectable } from '@angular/core';
import { Collation, Id, VehicleData } from "@models";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class VehicleService extends BaseApiService<VehicleData> {

  override baseUrl = 'fivepd:vehicle';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }
}
