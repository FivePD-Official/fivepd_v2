import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, CalloutDTO, Page, UnitDTO } from "@models";

@Injectable({
  providedIn: 'root'
})
export class CalloutService {

  constructor(
    private _http: HttpClient,
  ) {
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
