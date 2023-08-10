import { Injectable } from '@angular/core';
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { Router } from "@angular/router";
import { disableDispatchMode, enableDispatchMode } from "@store/app.actions";
import { HttpClient } from "@angular/common/http";
import { DispatcherCalloutDTOClientWrapper } from "../../models/callout";
import { Observable } from "rxjs";
import { ActionResponse } from "@models";

@Injectable({
  providedIn: 'root'
})
export class DispatchService {

  constructor(
    private readonly _store: Store<GlobalState>,
    private readonly _router: Router,
    private readonly _http: HttpClient,
  ) {
  }

  enable(): void {
    this._store.dispatch(enableDispatchMode());
    this._router.navigate(['mdt', 'dispatch']);
    this._http.post('fivepd:ui:dispatch:enable', {}).subscribe();
  }

  disable(): void {
    this._store.dispatch(disableDispatchMode());
    this._router.navigate(['mdt', 'dashboard']);
    this._http.post('fivepd:ui:dispatch:disable', {}).subscribe();
  }

  dismissCallout(callout: DispatcherCalloutDTOClientWrapper): Observable<void> {
    return this._http.post<void>('fivepd:dispatch:dismissCallout', {
      Id: callout.Callout.Id
    });
  }
}
