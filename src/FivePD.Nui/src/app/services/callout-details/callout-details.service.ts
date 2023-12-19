import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, CalloutDetails, CalloutDetailsWithDepartments, Collation, Id, Page } from "@models";
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class CalloutDetailsService {

  constructor(private readonly _http: HttpClient) {
  }

  get(id: Id): Observable<CalloutDetailsWithDepartments> {
    return this._http.post<CalloutDetailsWithDepartments>('fivepd:calloutDetails:get', {
      Id: parseId(id),
    });
  }

  getAll(collation?: Collation): Observable<Page<CalloutDetails>> {
    return this._http.post<Page<CalloutDetails>>('fivepd:calloutDetails:getAllPageable', collation);
  }

  updateRuleset(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:calloutDetails:updateRuleset', request);
  }
}
