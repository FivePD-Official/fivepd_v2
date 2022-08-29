import { Action, Selector, State, StateContext } from '@ngxs/store';
import { Injectable } from '@angular/core';
import { User } from '@models';
import { SetUser, TogglePanicMode } from './user.actions';

export interface UserStateModel {
  isPanicModeActive: boolean,
  details: User,
}

@State<UserStateModel>({
  name: 'user',
  defaults: {
    isPanicModeActive: false,
    details: {} as User,
  }
})

@Injectable()
export class UserState {

  constructor() {}

  @Selector([UserState])
  static details(state: UserStateModel): User {
    return state.details;
  }

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
