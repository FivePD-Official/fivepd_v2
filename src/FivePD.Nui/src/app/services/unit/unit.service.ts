import { Injectable } from '@angular/core';
import { ActionResponse, Id, UnitDTO } from "@models";
import { Observable } from "rxjs";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class UnitService extends BaseApiService<UnitDTO> {

  override baseUrl = 'fivepd:unit';

  get(id: Id) {
    return this._get('NOT_IMPLEMENTED', id); // TODO
  }

  getPage() {
    return this._getPage('getAll');
  }

  join(unit: UnitDTO): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl('join'), {
      Callsign: unit.Callsign,
    });
  }

  requestToJoin(unit: UnitDTO): Observable<void> {
    return this.httpClient.post<void>(this.createUrl('requestToJoin'), {
      Callsign: unit.Callsign,
    });
  }

  leave(unit: UnitDTO): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl('leave'), {
      Callsign: unit.Callsign,
    });
  }
}
