import { createReducer, on } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';
import { setHasShownWelcomeModal, setUser } from '@store/user.actions';

export interface UserStateModel {
  details: PlayerDetailsDTO;
  hasShownWelcomeModal: boolean;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
  hasShownWelcomeModal: false,
};

export const userReducer = createReducer(
  initialState,
  on(setUser, (state, action) => {
    return {...state, details: action.payload};
  }),
  on(setHasShownWelcomeModal, (state, action) => {
    return {...state, hasShownWelcomeModal: action.payload};
  }),
);
