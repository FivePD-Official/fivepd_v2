import { Injectable } from '@angular/core';
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { Router } from "@angular/router";
import { disableDispatchMode, enableDispatchMode } from "@store/app.actions";
import { HttpClient } from "@angular/common/http";
import { CalloutRemoveFromDispatcher, DispatcherCalloutDTOClientWrapper } from "../../models/callout";
import { BehaviorSubject, Observable } from "rxjs";
import { EventService } from "@services/event/event.service";
import { NuiEventType } from "@enums";
import { ConfigService } from "@services/config/config.service";

export type Callout = DispatcherCalloutDTOClientWrapper & {
  timeout: number;
  generatedAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class DispatchService {

  readonly incomingCallouts$ = new BehaviorSubject<Callout[]>([]);
  maxTimeout: number = 0;

  constructor(
    private readonly _store: Store<GlobalState>,
    private readonly _router: Router,
    private readonly _http: HttpClient,
    private readonly _eventService: EventService,
    private readonly _configService: ConfigService,
  ) {
    _configService.getGeneralConfig().subscribe(config => {
      this.maxTimeout = Math.round(config.config.Callout.DispatchTimeoutInMs / 1000);
    });

    _eventService
      .onEvent<DispatcherCalloutDTOClientWrapper>(NuiEventType.SendCalloutToDispatcher)
      .subscribe(callout => this.onCalloutReceived(callout));

    _eventService
      .onEvent<CalloutRemoveFromDispatcher>(NuiEventType.RemoveCalloutFromDispatcher)
      .subscribe(event => this.onCalloutRemove(event.CalloutId));

    this.updateCalloutTimeouts();
    setInterval(() => {
      this.updateCalloutTimeouts();
    }, 1000);
  }

  getActiveCallouts(): void {
    this._http.post<DispatcherCalloutDTOClientWrapper[]>('fivepd:ui:dispatch:getActiveCallouts', {}).subscribe(callouts => {
      this.incomingCallouts$.next(callouts.map(c => this.mapCallout(c)));
    });
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

  private onCalloutReceived(callout: DispatcherCalloutDTOClientWrapper): void {
    this.incomingCallouts$.next([...this.incomingCallouts$.value, this.mapCallout(callout)]);
  }

  private onCalloutRemove(calloutId: string): void {
    this.incomingCallouts$.next([...this.incomingCallouts$.value.filter(x => x.Callout.Id !== calloutId)]);
  }

  private mapCallout(callout: DispatcherCalloutDTOClientWrapper): Callout {
    const generatedAt = new Date(callout.Callout.GeneratedAt);
    return {
      ...callout,
      timeout: this.calculateTimeout(callout as Callout),
      generatedAt,
    };
  }

  private updateCalloutTimeouts(): void {
    for (const callout of this.incomingCallouts$.value) {
      callout.timeout = this.calculateTimeout(callout);
    }
  }

  private calculateTimeout(callout: Callout): number {
    const generatedAt = callout.generatedAt ?? new Date(callout.Callout.GeneratedAt);
    return this.maxTimeout - Math.round(this.calculateSecondsDifference(generatedAt, new Date()));
  }

  private calculateSecondsDifference(date1: Date, date2: Date): number {
    const millisecondsDifference = Math.abs((date2 as any) - (date1 as any));
    return millisecondsDifference / 1000;
  }
}
