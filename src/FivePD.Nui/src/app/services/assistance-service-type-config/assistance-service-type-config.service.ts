import { Injectable } from '@angular/core';
import { ActionResponse, AssistanceServiceTypeConfig, Collation, Id } from '@models';
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class AssistanceServiceTypeConfigService extends BaseApiService<AssistanceServiceTypeConfig> {

  override baseUrl = 'fivepd:assistanceService';

  get(id: Id) {
    return this._get('getTypeConfig', id);
  }

  getPage(collation?: Collation) {
    return this._getPage('getTypeConfigPage', collation);
  }

  edit(request: object) {
    return this.httpClient.post<ActionResponse>(this.createUrl('editTypeConfig'), request);
  }
}
