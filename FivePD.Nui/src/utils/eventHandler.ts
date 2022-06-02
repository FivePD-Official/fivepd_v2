import type { IBaseNuiEvent } from '@interfaces';
import type { NuiEventType } from '@enums';
import { Subject, filter, Observable } from 'rxjs';

const eventHandler = new Subject<IBaseNuiEvent>();

export function nuiEventNext(data: IBaseNuiEvent) {
  eventHandler.next(data);
}

export function onNuiEvent<T>(...events: NuiEventType[]): Observable<T> {
  return eventHandler
    .pipe(
      filter((event: IBaseNuiEvent) => (events as string[]).includes(event.type))
    ) as any as Observable<T>;
}
