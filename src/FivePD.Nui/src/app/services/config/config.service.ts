import { Injectable } from '@angular/core';
import { GeneralConfig, GetSafeZoneSize, IGeneralConfig, User } from "@models";
import { HttpClient } from "@angular/common/http";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { map, Observable, of, tap } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class ConfigService {

  private user?: User;
  private generalSettings?: GeneralConfig;

  safeZoneSize: number = -1;

  constructor(
    private _http: HttpClient,
    private readonly store: Store<GlobalState>,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details.User);
    this.getSafeZoneSize().subscribe(safeZoneSize => {
      this.safeZoneSize = safeZoneSize;
    })
  }

  getSafeZoneSize(): Observable<number> {
    if (this.safeZoneSize > -1) {
      return of(this.safeZoneSize);
    }

    return this._http.post<GetSafeZoneSize>('fivepd:config:getSafeZoneSize', {})
      .pipe(
        map(data => 1 - data.SafeZoneSize),
        map(safeZoneSize => safeZoneSize < 0 ? 0 : safeZoneSize),
      );
  }

  getGeneralSettings(): Observable<GeneralConfig> {
    if (this.generalSettings) {
      return of(this.generalSettings);
    }

    return this._http.post<IGeneralConfig>('fivepd:config:getGeneralSettings', {})
      .pipe(
        map(settings => new GeneralConfig(settings)),
        tap(settings => this.generalSettings = settings),
      );
  }
}
