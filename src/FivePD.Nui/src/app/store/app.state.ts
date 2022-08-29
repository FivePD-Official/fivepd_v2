import { Action, Selector, State, StateContext } from '@ngxs/store';
import { Injectable } from '@angular/core';
import { SetAvailableLocales, SetMdtPath, ToggleSidebarCollapse } from './app.actions';

export interface IListItem {
  Key: number;
  Title: string;
}

export interface AppStateModel {
  availableLocales: IListItem[];
  isSidebarCollapsed: boolean;
  previousMdtPath: string[];
}

@State<AppStateModel>({
  name: 'app',
  defaults: {
    availableLocales: [],
    isSidebarCollapsed: false,
    previousMdtPath: ['mdt', 'dashboard'],
  }
})

@Injectable()
export class AppState {

  constructor() {}

  @Selector([AppState])
  static isSidebarCollapsed(state: AppStateModel): boolean {
    return state.isSidebarCollapsed;
  }

  @Selector([AppState])
  static previousMdtPath(state: AppStateModel): string[] {
    return state.previousMdtPath;
  }

  @Action(SetAvailableLocales)
  setAvailableLocales({ patchState, dispatch }: StateContext<AppStateModel>, { payload }: SetAvailableLocales) {
    patchState({
      availableLocales: payload
    });
  }

  @Action(ToggleSidebarCollapse)
  toggleSidebarCollapse({ patchState, dispatch }: StateContext<AppStateModel>, { payload }: ToggleSidebarCollapse) {
    patchState({
      isSidebarCollapsed: payload
    });
  }

  @Action(SetMdtPath)
  setMdtPath({ patchState, dispatch }: StateContext<AppStateModel>, { payload }: SetMdtPath) {
    patchState({
      previousMdtPath: payload
    });
  }
}
