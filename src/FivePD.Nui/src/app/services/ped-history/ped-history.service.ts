import { inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, Id, Page, PedData, SimplePedHistory } from "@models";
import { parseId } from "../../helpers/parse-id";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class PedHistoryService extends BaseApiService<PedData> {

  override baseUrl = 'fivepd:pedHistory';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    if (collation && collation.Filters['Gender'] === undefined) {
      collation.Filters['Gender'] = -1;
    }

    return this._getPage('getAllPageable', collation);
  }

  getAllSimple(): Observable<SimplePedHistory[]> {
    return this.httpClient.post<SimplePedHistory[]>(this.createUrl('getAllSimple'), {});
  }
}
