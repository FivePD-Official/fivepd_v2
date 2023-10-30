import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, LooseDataEntity, Page, PedHistory, SimplePedHistory } from "@models";

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

  // TODO: refactor these 2 methods, move to LooseDataService
  getTickets(id: string | number): Observable<LooseDataEntity[]> {
    if (typeof id === 'string') {
      id = parseInt(id);
    }

    return this._http.post<LooseDataEntity[]>('fivepd:pedHistory:getTickets', {
      Id: id,
    });
  }

  getWarnings(id: string | number): Observable<LooseDataEntity[]> {
    if (typeof id === 'string') {
      id = parseInt(id);
    }

    return this._http.post<LooseDataEntity[]>('fivepd:pedHistory:getWarnings', {
      Id: id,
    });
  }

}
