import { Injectable } from '@angular/core';
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { CalloutRemoveFromDispatcher, DispatcherCalloutDTOClientWrapper } from "../../models/callout";
import { BehaviorSubject, Observable } from "rxjs";
import { EventService } from "@services/event/event.service";
import { ConfigService } from "@services/config/config.service";
import {
  CommonDispatcherUnitDTO,
  DispatcherUnitDTO,
  OnUnitCreated,
  OnUnitRemoved,
  SimplePlayer,
  UpdatePlayersEvent
} from "@models";
import { AppActions } from "@store/app.actions";

export type Callout = DispatcherCalloutDTOClientWrapper & {
  timeout: number;
  generatedAt: Date;
  units: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DispatchService {

  readonly incomingCallouts$ = new BehaviorSubject<Callout[]>([]);
  readonly activeCallouts$ = new BehaviorSubject<Callout[]>([]);
  readonly units$ = new BehaviorSubject<DispatcherUnitDTO[]>([]);
  readonly players$ = new BehaviorSubject<SimplePlayer[]>([]);

  maxTimeout: number = 0;

  constructor(
    private readonly _store: Store<GlobalState>,
    private readonly _router: Router,
    private readonly _http: HttpClient,
    private readonly _eventService: EventService,
    private readonly _configService: ConfigService,
  ) {
    _configService.getGeneralConfig().subscribe(config => {
      this.maxTimeout = Math.round(config.config.Callout.AutoRemove.TimeoutByDispatchType.Manual / 1000);

      if (config.config.Callout.AutoRemove.Enabled) {
        this.updateCalloutTimeouts();
        setInterval(() => {
          this.updateCalloutTimeouts();
        }, 1000);
      }
    });

    _eventService
      .onEvent<DispatcherCalloutDTOClientWrapper>('fivepd:dispatch:sendCalloutToDispatcher')
      .subscribe(callout => this.onCalloutReceived(callout));

    _eventService
      .onEvent<CalloutRemoveFromDispatcher>('fivepd:dispatch:removeCalloutFromDispatcher')
      .subscribe(event => this.onCalloutRemove(event.CalloutId));

    _eventService
      .onEvent<UpdatePlayersEvent>('fivepd:ui:dispatch:updatePlayers')
      .subscribe(event => this.onPlayersUpdate(event));

    _eventService
      .onEvent<OnUnitCreated>('fivepd:dispatch:onUnitCreated')
      .subscribe(event => this.onUnitCreated(event.Unit));

    _eventService
      .onEvent<OnUnitRemoved>('fivepd:dispatch:onUnitRemoved')
      .subscribe(event => this.onUnitRemoved(event.Callsign));
  }

  getIncomingCallouts(): void {
    this._http.post<DispatcherCalloutDTOClientWrapper[]>('fivepd:ui:dispatch:getCallouts', {HasUnitsAssigned: false}).subscribe(callouts => {
      this.incomingCallouts$.next(callouts.map(c => this.mapCallout(c)));
    });
  }

  getActiveCallouts(): void {
    this._http.post<DispatcherCalloutDTOClientWrapper[]>('fivepd:ui:dispatch:getCallouts', {HasUnitsAssigned: true}).subscribe(callouts => {
      this.activeCallouts$.next(callouts.map(c => this.mapCallout(c)));
    });
  }

  getUnits(): void {
    this._http.post<DispatcherUnitDTO[]>('fivepd:ui:dispatch:getUnits', {}).subscribe(units => {
      this.units$.next(units);
    });
  }

  enable(): void {
    this._store.dispatch(AppActions.toggleDispatchMode({state: true}));
    this._router.navigate(['mdt', 'dispatch']);
    this._http.post('fivepd:ui:dispatch:enable', {}).subscribe();
  }

  disable(): void {
    this._store.dispatch(AppActions.toggleDispatchMode({state: false}));
    this._router.navigate(['mdt', 'dashboard']);
    this._http.post('fivepd:ui:dispatch:disable', {}).subscribe();
  }

  dismissCallout(callout: DispatcherCalloutDTOClientWrapper): Observable<void> {
    return this._http.post<void>('fivepd:dispatch:dismissCallout', {
      Id: callout.Callout.Id
    });
  }

  assignCalloutToUnits(unitCallsigns: string[], callout: Callout) {
    if (!this.activeCallouts$.value.some(x => x.Callout.Id === callout.Callout.Id)) {
      this.activeCallouts$.next([...this.activeCallouts$.value, {...callout, units: [...unitCallsigns]}]);
      this.incomingCallouts$.next([...this.incomingCallouts$.value.filter(x => x.Callout.Id !== callout.Callout.Id)]);
    }

    return this._http.post<void>('fivepd:dispatch:assignCalloutToUnits', {
      UnitCallsigns: unitCallsigns,
      CalloutId: callout.Callout.Id
    });
  }

  private onCalloutReceived(callout: DispatcherCalloutDTOClientWrapper): void {
    this.incomingCallouts$.next([...this.incomingCallouts$.value, this.mapCallout(callout)]);
  }

  private onCalloutRemove(calloutId: string): void {
    this.incomingCallouts$.next([...this.incomingCallouts$.value.filter(x => x.Callout.Id !== calloutId)]);
  }

  private onPlayersUpdate(event: UpdatePlayersEvent): void {
    this.players$.next(event.Players);
  }

  private onUnitCreated(unit: CommonDispatcherUnitDTO): void {
    this.units$.next([...this.units$.value, unit]);
  }

  private onUnitRemoved(callsign: string): void {
    this.units$.next([...this.units$.value.filter(x => x.Callsign !== callsign)]);
  }

  private mapCallout(callout: DispatcherCalloutDTOClientWrapper): Callout {
    const generatedAt = new Date(callout.Callout.GeneratedAt);
    return {
      ...callout,
      timeout: this.calculateTimeout(generatedAt),
      generatedAt,
      units: [],
    };
  }

  private updateCalloutTimeouts(): void {
    for (const callout of this.incomingCallouts$.value) {
      const calculatedTimeout = this.calculateTimeout(callout.generatedAt);
      callout.timeout = calculatedTimeout < 0 ? 0 : calculatedTimeout;
    }
  }

  private calculateTimeout(generatedAt: Date): number {
    return this.maxTimeout - Math.round(this.calculateSecondsDifference(generatedAt, new Date()));
  }

  private calculateSecondsDifference(date1: Date, date2: Date): number {
    const millisecondsDifference = Math.abs((date2 as any) - (date1 as any));
    return millisecondsDifference / 1000;
  }
}
