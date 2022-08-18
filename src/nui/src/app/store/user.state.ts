import { Action, State, StateContext } from '@ngxs/store';
import { Injectable } from '@angular/core';
import { IUser } from '@interfaces';
import { SetUser, TogglePanicMode } from './user.actions';

export interface UserStateModel {
  isPanicModeActive: boolean,
  details: IUser,
}

@State<UserStateModel>({
  name: 'user',
  defaults: {
    isPanicModeActive: false,
    details: {} as IUser,
  }
})

@Injectable()
export class UserState {

  constructor() {}

  @Action(SetUser)
  setUser({ patchState, dispatch }: StateContext<UserStateModel>, { payload }: SetUser) {
    patchState({
      details: payload
    });
  }

  @Action(TogglePanicMode)
  togglePanicMode({ patchState, dispatch }: StateContext<UserStateModel>, { payload }: TogglePanicMode) {
    patchState({
      isPanicModeActive: payload
    });
  }
}
