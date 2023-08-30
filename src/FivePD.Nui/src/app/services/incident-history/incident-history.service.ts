import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { SimpleIncidentHistory } from "@models";
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

  getAllOfUser(): Observable<SimpleIncidentHistory[]> {
    return this._http.post<SimpleIncidentHistory[]>('fivepd:incidentHistory:getAllOfUser', {
      Id: this.userId,
    });
  }
}
