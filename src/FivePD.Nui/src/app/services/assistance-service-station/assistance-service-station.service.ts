import { Injectable } from '@angular/core';
import { ActionResponse, AssistanceServiceStation, AssistanceServiceTypeConfig, Collation, Id } from '@models';
import { BaseApiService } from "@services/api/base-api/base-api.service";
import { CrudApiService } from "@services/api/crud-api/crud-api.service";
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AssistanceServiceStationService extends CrudApiService<AssistanceServiceStation> {
  override baseUrl = 'fivepd:assistanceService';

  get(id: Id) {
    return this._get('getStation', id);
  }

  getPage(collation?: Collation) {
    return this._getPage('getStationPage', collation);
  }

  create(request: object) {
    return this.httpClient.post<ActionResponse>(this.createUrl('createStation'), request);
  }

  delete(id: Id) {
    return this._delete('deleteStation', id);
  }

  edit(request: object) {
    return this._edit('edit', request);
  }
}
