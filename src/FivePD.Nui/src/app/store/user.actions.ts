import { createAction, props } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';

export const setUser = createAction('[user] set user', props<{ payload: PlayerDetailsDTO }>());
