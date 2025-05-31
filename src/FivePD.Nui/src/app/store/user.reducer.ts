import { createReducer, on } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';
import { UserActions } from '@store/user.actions';

export interface UserStateModel {
  details: PlayerDetailsDTO;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
};

export const userReducer = createReducer(
  initialState,
  on(UserActions.setUser, (state, action) => ({...state, details: action.user})),
);
