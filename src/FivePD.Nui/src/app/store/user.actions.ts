import { createAction, props } from '@ngrx/store';
import { InformationUpdateDTO, PlayerDetailsDTO } from '@models';

export const setUser = createAction('[user] set user', props<{ payload: PlayerDetailsDTO }>());

export const setHasShownWelcomeModal = createAction('[user] set has shown welcome modal', props<{ payload: boolean }>());

export const setInformation = createAction('[user] set information', props<{ payload: InformationUpdateDTO }>());
