import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { CalloutDTO, ClientCalloutWrapper, Page } from "@models";

@Injectable({
  providedIn: 'root'
})
export class CalloutService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  getActive(): Observable<ClientCalloutWrapper> {
    return this._http.post<ClientCalloutWrapper>('fivepd:ui:callout:getActive', {});
  }

  getAll(): Observable<Page<CalloutDTO>> {
    return this._http.post<Page<CalloutDTO>>('fivepd:callout:getAllActiveUI', {});
  }

  // join(unit: UnitDTO): Observable<ActionResponse> {
  //   return this._http.post<ActionResponse>('fivepd:unit:join', {
  //     Callsign: unit.Callsign,
  //   });
  // }
}
