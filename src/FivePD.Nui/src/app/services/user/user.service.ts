import { Injectable } from '@angular/core';
import { ActionResponse, User } from '@models';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { setUser } from '@store/user.actions';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(
    private _http: HttpClient,
    private readonly store: Store<GlobalState>,
  ) { }

  refreshData(): void {
    this._http.post<User>('fivepd:player:getDetails', {}).subscribe(user => {
      this.store.dispatch(setUser({payload: user}));
    });
  }

  changeRank(rank: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeRank', rank);
  }

  changeDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeDivision', division);
  }
}
