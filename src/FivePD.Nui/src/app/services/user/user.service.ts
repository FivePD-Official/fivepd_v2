import { Injectable } from '@angular/core';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { SetUser } from '@store/user.actions';
import { ActionResponse, User } from '@models';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  @Dispatch() setUser = (user: User) => new SetUser(user);

  constructor(
    private _http: HttpClient,
  ) { }

  refreshData(): void {
    this._http.post<User>('fivepd:player:getDetails', {}).subscribe(res => {
      this.setUser(res);
    });
  }

  changeRank(rank: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeRank', rank);
  }

  changeDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeDivision', division);
  }
}
