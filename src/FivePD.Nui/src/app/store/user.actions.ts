import { createAction, props } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';

export const setUser = createAction('[user] set user', props<{ payload: PlayerDetailsDTO }>());
export const setHasShownWelcomeModal = createAction('[user] set has shown welcome modal', props<{ payload: boolean }>());
