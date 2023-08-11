import { createAction, props } from '@ngrx/store';
import { CoordinateWithDate } from '@store/app.reducer';

export const toggleSidebarCollapse = createAction('[app] toggle sidebar collapse');

export const setMdtPath = createAction('[app] set mdt path', props<{ payload: string[] }>());

export const addDevToolsCoordinate = createAction('[app] add dev tools coordinates', props<{
  payload: CoordinateWithDate
}>());

export const clearDevToolsCoordinates = createAction('[app] clear dev tools coordinates');

export const enableDispatchMode = createAction('[app] enable dispatch mode');

export const disableDispatchMode = createAction('[app] disable dispatch mode');

export const dispatchModeMapZoom = createAction('[app] dispatch mode map zoom', props<{ payload: number }>());

export const setWindowMaximized = createAction('[app] set window maximized', props<{ payload: boolean }>());
