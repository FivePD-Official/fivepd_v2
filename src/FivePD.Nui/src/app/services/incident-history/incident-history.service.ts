import { inject, Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { Collation, IncidentHistory, SimpleIncidentHistory } from "@models";
import { GlobalState } from "@store";
import { Store } from "@ngrx/store";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class IncidentHistoryService extends BaseApiService<IncidentHistory> {

  override baseUrl = 'fivepd:incidentHistory';

  private readonly _store = inject<Store<GlobalState>>(Store);
  private userId!: number;

  constructor() {
    super();
    this._store.select('user', 'details', 'User', 'Id').subscribe(userId => this.userId = userId);
  }

  get(id: string) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }

  getAllOfUser(): Observable<SimpleIncidentHistory[]> {
    return this.httpClient.post<SimpleIncidentHistory[]>(this.createUrl('getAllOfUser'), {
      Id: this.userId,
    });
  }
}
