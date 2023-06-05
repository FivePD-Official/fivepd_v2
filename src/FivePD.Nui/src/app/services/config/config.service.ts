import { Injectable } from '@angular/core';
import { GeneralSettings, IGeneralSettings, User } from "@models";
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
  private generalSettings?: GeneralSettings;

  constructor(
    private _http: HttpClient,
    private readonly store: Store<GlobalState>,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details.User);
  }

  getGeneralSettings(): Observable<GeneralSettings> {
    if (this.generalSettings) {
      return of(this.generalSettings);
    }

    return this._http.post<IGeneralSettings>('fivepd:config:getGeneralSettings', {})
      .pipe(
        map(settings => new GeneralSettings(settings)),
        tap(settings => this.generalSettings = settings),
      );
  }
}
