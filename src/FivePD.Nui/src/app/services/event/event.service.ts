import { Injectable } from '@angular/core';
import { filter, Observable, ReplaySubject } from 'rxjs';
import { BaseNuiEvent, User } from '@models';
import { NuiEventType } from '@enums';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EventService {

  hadFirstEvent = false;
  private _events$ = new ReplaySubject<BaseNuiEvent>(10);

  constructor(
    private _http: HttpClient,
    private _router: Router,
  ) { }

  next(data: BaseNuiEvent): void {
    if (!this.hadFirstEvent) {
      this.hadFirstEvent = true;
      document.body.style.display = 'none';
      this._http.post(NuiEventType.Close, {}).subscribe();
    }

    if (!environment.production) {
      console.log(data)
    }

    this._events$.next(data);
  }

  onEvent<T>(...events: NuiEventType[]): Observable<T> {
    return this._events$
      .pipe(
        filter((event: BaseNuiEvent) => (events as string[]).includes(event.type))
      ) as any as Observable<T>;
  }
}
