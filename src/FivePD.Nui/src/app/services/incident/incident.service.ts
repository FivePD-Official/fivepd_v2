import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { ActionResponse, Id, IncidentDTO, IncidentWrapper } from "@models";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class IncidentService extends BaseApiService<IncidentDTO> {

  override baseUrl = 'fivepd:incident';

  get(id: Id) {
    return this._get('NOT_IMPLEMENTED', id); // TODO
  }

  getPage() {
    return this._getPage('getAllActive');
  }

  getAssignedIncident(): Observable<IncidentWrapper> {
    return this.httpClient.post<IncidentWrapper>(this.createUrl('getAssigned'), {});
  }


  selfAssign(incident: IncidentDTO): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl('selfAssign'), {
      Id: incident.Id,
    });
  }

}
