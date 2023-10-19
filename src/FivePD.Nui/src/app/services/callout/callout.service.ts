import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, IncidentDTO, CalloutTypeDTO, IncidentWrapper, Page } from "@models";

@Injectable({
  providedIn: 'root'
})
export class CalloutService {

  constructor(
    private _http: HttpClient,
  ) {
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
