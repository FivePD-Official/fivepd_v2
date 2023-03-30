import { createAction, props } from '@ngrx/store';
import { CoordinateWithDate, IListItem } from '@store/app.reducer';

export const toggleSidebarCollapse = createAction('[app] toggle sidebar collapse');
export const setMdtPath = createAction('[app] set mdt path', props<{ payload: string[] }>());
export const addDevToolsCoordinate = createAction('[app] add dev tools coordinates', props<{ payload: CoordinateWithDate }>());
export const clearDevToolsCoordinates = createAction('[app] clear dev tools coordinates');
