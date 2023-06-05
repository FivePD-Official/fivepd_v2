import { Injectable } from '@angular/core';
import { Page, PlayerDetailsDTO, UnitDTO } from "@models";
import { HttpClient } from "@angular/common/http";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class UnitService {

  user?: PlayerDetailsDTO;

  constructor(
    private _http: HttpClient,
    private readonly store: Store<GlobalState>,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details);
  }

  getAll(): Observable<Page<UnitDTO>> {
    return this._http.post<Page<UnitDTO>>('fivepd:unit:getAll', {});
  }
}
