import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionResponse, Collation, Id, Page, VehicleConfig, VehicleConfigType } from '@models';
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class VehicleConfigService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  get(id: Id): Observable<VehicleConfig> {
    return this._http.post<VehicleConfig>('fivepd:ui:vehicleConfig:get', {Id: parseId(id)});
  }

  getAllPageable(type: VehicleConfigType, collation?: Collation): Observable<Page<VehicleConfig>> {
    return this._http.post<Page<VehicleConfig>>('fivepd:ui:vehicleConfig:getAllPageable', {
      ...collation,
      Filters: {
        Type: type,
        ...collation?.Filters,
      },
    });
  }

  create(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:vehicleConfig:create', request);
  }

  edit(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:vehicleConfig:edit', request);
  }

  delete(id: Id): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:vehicleConfig:delete', {Id: parseId(id)});
  }
}
