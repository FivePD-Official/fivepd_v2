import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, CalloutDTO, CalloutTypeDTO, ClientCalloutWrapper, Page } from "@models";

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

  selfAssign(callout: CalloutDTO): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:callout:selfAssign', {
      Id: callout.Id,
    });
  }

  getAllTypes(): Observable<Page<CalloutTypeDTO>> {
    return this._http.post<Page<CalloutTypeDTO>>('fivepd:ui:callout:getAllTypes', {});
  }

  dispatch(callout: CalloutTypeDTO): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:callout:dispatch', {
      TypeName: callout.TypeName,
    });
  }
}
