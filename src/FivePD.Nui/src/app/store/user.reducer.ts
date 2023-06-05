import { createReducer, on } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';
import { setUser } from '@store/user.actions';

export interface UserStateModel {
  details: PlayerDetailsDTO;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
};

export const userReducer = createReducer(
  initialState,
  on(setUser, (state, action) => {
    return {...state, details: action.payload};
  }),
);
