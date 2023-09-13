import { Injectable } from '@angular/core';
import {
  ActionResponse,
  Collation,
  Page,
  PlayerDetailsDTO,
  SimplePedHistory,
  SimplePlayer,
  SimpleUser,
  User
} from '@models';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { setUser } from '@store/user.actions';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export enum UserRole {
  None = 0,
  Admin = 1,
  Developer = 2,
}

@Injectable({
  providedIn: 'root'
})
export class UserService {

  user?: PlayerDetailsDTO;

  constructor(
    private _http: HttpClient,
    private readonly store: Store<GlobalState>,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details);
  }

  getAllSimple(): Observable<SimpleUser[]> {
    return this._http.post<SimpleUser[]>('fivepd:user:getAllSimple', {});
  }

  dismissWelcomeModal(): Observable<void> {
    return this._http.post<void>('fivepd:player:dismissedWelcomeModal', {});
  }

  userHasRole(...roles: UserRole[]): boolean {
    if (!this.user?.User) {
      return false;
    }

    for (const role of roles) {
      const hasRole = !!(this.user.User.Role & role)
      if (hasRole) {
        return true;
      }
    }

    return false;
  }

  refreshData(): void {
    this._http.post<PlayerDetailsDTO>('fivepd:player:getDetails', {}).subscribe(user => {
      this.store.dispatch(setUser({payload: user}));
    });
  }

  changeRank(rank: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeRank', rank);
  }

  changeDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeDivision', division);
  }

  changeFixedRank(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeFixedRank', request);
  }

  getAllPageable(collation: Collation): Observable<Page<User>> {
    return this._http.post<Page<User>>('fivepd:player:getAllPageable', {...collation});
  }

  changeRole(user: User, roles: number[]): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:player:changeRole', {
      UserId: user.Id,
      Roles: roles,
    });
  }
}
