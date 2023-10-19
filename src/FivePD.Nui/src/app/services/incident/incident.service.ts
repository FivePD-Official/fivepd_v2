import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { IncidentWrapper, IncidentDTO, Page, ActionResponse } from "@models";

@Injectable({
  providedIn: 'root'
})
export class IncidentService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  getAssignedIncident(): Observable<IncidentWrapper> {
    return this._http.post<IncidentWrapper>('fivepd:incident:getAssigned', {});
  }

  getAll(): Observable<Page<IncidentDTO>> {
    return this._http.post<Page<IncidentDTO>>('fivepd:incident:getAllActive', {});
  }

  selfAssign(incident: IncidentDTO): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:incident:selfAssign', {
      Id: incident.Id,
    });
  }

}
