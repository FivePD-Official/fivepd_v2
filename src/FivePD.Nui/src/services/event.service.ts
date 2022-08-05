import type { BaseNuiEvent } from '@interfaces';
import type { NuiEventType } from '@enums';
import { Subject, filter, Observable } from 'rxjs';

class EventService {
  
  private _events$ = new Subject<BaseNuiEvent>();
  
  next(data: BaseNuiEvent): void {
    this._events$.next(data);
  }
  
  onEvent<T>(...events: NuiEventType[]): Observable<T> {
    return this._events$
      .pipe(
        filter((event: BaseNuiEvent) => (events as string[]).includes(event.type))
      ) as any as Observable<T>;
  }
}

export const eventService = new EventService();