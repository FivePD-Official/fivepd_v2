import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, CalloutDetails, CalloutDetailsWithDepartments, Collation, Id, Page } from "@models";
import { parseId } from "../../helpers/parse-id";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class CalloutDetailsService extends BaseApiService<CalloutDetails> {

  override baseUrl = 'fivepd:calloutDetails';

  get(id: Id) {
    return this._get<CalloutDetailsWithDepartments>('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }

  updateRuleset(request: object): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl('updateRuleset'), request);
  }
}
