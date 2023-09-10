import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, Page, PedHistory, SimplePedHistory } from "@models";

@Injectable({
  providedIn: 'root'
})
export class PedHistoryService {

  constructor(private readonly _http: HttpClient) { }

  get(id: number | string): Observable<PedHistory> {
    if (typeof id === 'string') {
      id = parseInt(id);
    }

    return this._http.post<PedHistory>('fivepd:pedHistory:get', {
      Id: id,
    });
  }

  getAll(collation?: Collation): Observable<Page<PedHistory>> {
    if (collation && collation.Filters['Gender'] === undefined) {
      collation.Filters['Gender'] = -1;
    }

    return this._http.post<Page<PedHistory>>('fivepd:pedHistory:getAllPageable', collation);
  }

  getAllSimple(): Observable<SimplePedHistory[]> {
    return this._http.post<SimplePedHistory[]>('fivepd:pedHistory:getAllSimple', {});
  }

}
