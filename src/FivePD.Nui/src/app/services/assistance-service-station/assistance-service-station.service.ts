import { Injectable } from '@angular/core';
import {
  ActionResponse,
  AssistanceServiceStation,
  AssistanceServiceStationTypeConfig,
  AssistanceServiceTypeConfig,
  Collation,
  Id, Page
} from '@models';
import { BaseApiService } from "@services/api/base-api/base-api.service";
import { CrudApiService } from "@services/api/crud-api/crud-api.service";
import { Observable } from 'rxjs';
import { parseId } from "../../helpers/parse-id";

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
    return this._edit('NOT IMPLEMENTED', request);
  }

  getTypeConfigsPage(collation?: Collation) {
    return this._getPage<Page<AssistanceServiceStationTypeConfig>>('getStationTypeConfigsPage', collation);
  }

  addTypeConfig(request: object) {
    return this.httpClient.post<ActionResponse>(this.createUrl('addStationTypeConfig'), request);
  }

  editTypeConfig(request: object) {
    return this.httpClient.post<ActionResponse>(this.createUrl('editStationTypeConfig'), request);
  }

  deleteTypeConfig(id: Id) {
    return this._delete('deleteStationTypeConfig', id);
  }

  getAvailableStationTypeConfigs(id: Id) {
    return this.httpClient.post<AssistanceServiceTypeConfig[]>(this.createUrl('getAvailableStationTypeConfigs'), {Id: parseId(id)});
  }
}
