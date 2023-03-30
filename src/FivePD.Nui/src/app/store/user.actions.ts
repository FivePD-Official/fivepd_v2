import { createAction, props } from '@ngrx/store';
import { User } from '@models';

export const setUser = createAction('[user] set user', props<{ payload: User }>());
