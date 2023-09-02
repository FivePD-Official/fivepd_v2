import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, Department, IncidentHistory, Page, SimpleIncidentHistory } from "@models";
import { GlobalState } from "@store";
import { Store } from "@ngrx/store";

@Injectable({
  providedIn: 'root'
})
export class IncidentHistoryService {

  userId!: number;

  constructor(
    private readonly _http: HttpClient,
    private readonly _store: Store<GlobalState>,
  ) {
    _store.select('user', 'details', 'User', 'Id').subscribe(userId => this.userId = userId);
  }

  get(id: string): Observable<IncidentHistory> {
    return this._http.post<IncidentHistory>('fivepd:incidentHistory:get', {
      Id: id,
    });
  }

  getAll(collation?: Collation): Observable<Page<IncidentHistory>> {
    return this._http.post<Page<IncidentHistory>>('fivepd:incidentHistory:getAll', collation);
  }

  getAllOfUser(): Observable<SimpleIncidentHistory[]> {
    return this._http.post<SimpleIncidentHistory[]>('fivepd:incidentHistory:getAllOfUser', {
      Id: this.userId,
    });
  }
}
