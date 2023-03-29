import { Action, Selector, State, StateContext, Store } from '@ngxs/store';
import { Injectable } from '@angular/core';
import {
  AddDevToolsCoordinate,
  ClearDevToolsCoordinates,
  SetAvailableLocales,
  SetMdtPath,
  ToggleSidebarCollapse
} from './app.actions';
import { Page } from '@models';

export interface IListItem {
  Key: number;
  Title: string;
}

export interface Coordinate {
  X: string;
  Y: string;
  Z: string;
}

export interface WithDate {
  date: Date;
}

export type CoordinateWithDate = Coordinate & WithDate;

export interface DevToolsState {
  coordinates: Page<CoordinateWithDate>;
}

export interface AppStateModel {
  availableLocales: IListItem[];
  isSidebarCollapsed: boolean;
  previousMdtPath: string[];
  devTools: DevToolsState;
}

const devToolsDefaultState: DevToolsState = {
  coordinates: {
    PageSize: 10,
    PageCount: 1,
    CurrentPage: 0,
    ItemCount: 0,
    Items: []
  },
};

@State<AppStateModel>({
  name: 'app',
  defaults: {
    availableLocales: [],
    isSidebarCollapsed: false,
    previousMdtPath: ['mdt', 'dashboard'],
    devTools: devToolsDefaultState,
  }
})

@Injectable()
export class AppState {

  constructor(private readonly store: Store) {
  }

  @Selector([AppState])
  static isSidebarCollapsed(state: AppStateModel): boolean {
    return state.isSidebarCollapsed;
  }

  @Selector([AppState])
  static previousMdtPath(state: AppStateModel): string[] {
    return state.previousMdtPath;
  }

  @Selector([AppState])
  static devTools(state: AppStateModel): DevToolsState {
    return state.devTools;
  }

  @Selector([AppState])
  static devToolsCoordinates(state: AppStateModel): Page<CoordinateWithDate> {
    return state.devTools.coordinates;
  }

  @Action(SetAvailableLocales)
  setAvailableLocales({patchState, dispatch}: StateContext<AppStateModel>, {payload}: SetAvailableLocales) {
    patchState({
      availableLocales: payload
    });
  }

  @Action(ToggleSidebarCollapse)
  toggleSidebarCollapse({patchState, dispatch}: StateContext<AppStateModel>, {payload}: ToggleSidebarCollapse) {
    patchState({
      isSidebarCollapsed: payload
    });
  }

  @Action(SetMdtPath)
  setMdtPath({patchState, dispatch}: StateContext<AppStateModel>, {payload}: SetMdtPath) {
    patchState({
      previousMdtPath: payload
    });
  }

  @Action(AddDevToolsCoordinate)
  addDevToolsCoordinate({
                          patchState,
                          getState
                        }: StateContext<AppStateModel>, {payload}: AddDevToolsCoordinate) {
    const devToolsState = this.store.selectSnapshot(AppState.devTools);
    const coordinates = [...devToolsState.coordinates.Items, payload];

    patchState({
      devTools: {
        ...devToolsState,
        coordinates: {
          ...devToolsState.coordinates,
          PageSize: coordinates.length,
          Items: coordinates,
        }
      }
    });
  }

  @Action(ClearDevToolsCoordinates)
  clearDevToolsCoordinates({patchState}: StateContext<AppStateModel>) {
    const devToolsState = this.store.selectSnapshot(AppState.devTools);
    patchState({
      devTools: {
        ...devToolsState,
        ...devToolsDefaultState,
      }
    });
  }

}
