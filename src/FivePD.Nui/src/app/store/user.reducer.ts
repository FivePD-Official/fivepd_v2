import { createReducer, on } from '@ngrx/store';
import { InformationUpdateDTO, PlayerDetailsDTO } from '@models';
import { UserActions } from '@store/user.actions';

export interface UserStateModel {
  details: PlayerDetailsDTO;
  information: InformationUpdateDTO;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
  information: {} as InformationUpdateDTO,
};

export const userReducer = createReducer(
  initialState,
  on(UserActions.setUser, (state, action) => ({...state, details: action.user})),
  on(UserActions.setInformation, (state, action) => ({...state, information: action.information})),
);
