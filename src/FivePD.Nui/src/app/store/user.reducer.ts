import { createReducer, on } from '@ngrx/store';
import { User } from '@models';
import { setUser } from '@store/user.actions';

export interface UserStateModel {
  details: User;
}

const initialState: UserStateModel = {
  details: {} as User,
};

export const userReducer = createReducer(
  initialState,
  on(setUser, (state, action) => {
    return {...state, details: action.payload};
  }),
);
