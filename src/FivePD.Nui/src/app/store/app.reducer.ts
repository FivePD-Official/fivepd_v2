import { Page, PedHistory } from '@models';
import { createReducer, on } from '@ngrx/store';
import {
  addDevToolsCoordinate,
  clearDevToolsCoordinates, disableDispatchMode, dispatchModeMapZoom, enableDispatchMode, setWindowMaximized,
  setMdtPath,
  toggleSidebarCollapse, addPedToRecentlyViewed
} from '@store/app.actions';

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
  isSidebarCollapsed: boolean;
  previousMdtPath: string[];
  devTools: DevToolsState;
  dispatchMode: boolean;
  dispatchModeMapZoom: number;
  windowMaximized: boolean;
  recentlyViewedPeds: PedHistory[];
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

const initialState: AppStateModel = {
  isSidebarCollapsed: false,
  previousMdtPath: ['mdt', 'dashboard'],
  devTools: devToolsDefaultState,
  dispatchMode: false,
  dispatchModeMapZoom: 3,
  windowMaximized: false,
  recentlyViewedPeds: [],
};

export const appReducer = createReducer(
  initialState,
  on(toggleSidebarCollapse, (state) => {
    return {...state, isSidebarCollapsed: !state.isSidebarCollapsed};
  }),
  on(setMdtPath, (state, action) => {
    return {...state, previousMdtPath: action.payload};
  }),
  on(addDevToolsCoordinate, (state, action) => {
    const _state: AppStateModel = {
      ...state,
      devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}
    };
    const coordinates = [...state.devTools.coordinates.Items, action.payload];
    _state.devTools.coordinates.Items = coordinates;
    _state.devTools.coordinates.PageSize = coordinates.length;
    return _state;
  }),
  on(clearDevToolsCoordinates, (state) => {
    const _state: AppStateModel = {
      ...state,
      devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}
    };
    _state.devTools.coordinates.Items = [];
    _state.devTools.coordinates.PageSize = 0;
    return _state;
  }),
  on(enableDispatchMode, (state) => {
    return {...state, dispatchMode: true};
  }),
  on(disableDispatchMode, (state) => {
    return {...state, dispatchMode: false};
  }),
  on(dispatchModeMapZoom, (state, action) => {
    return {...state, dispatchModeMapZoom: action.payload};
  }),
  on(dispatchModeMapZoom, (state, action) => {
    return {...state, dispatchModeMapZoom: action.payload};
  }),
  on(setWindowMaximized, (state, action) => {
    return {...state, windowMaximized: action.payload};
  }),
  on(addPedToRecentlyViewed, (state, action) => {
    if (state.recentlyViewedPeds.some(x => x.Id === action.payload.Id)) {
      return state;
    }

    const recentlyViewedPeds = [action.payload, ...state.recentlyViewedPeds];
    if (recentlyViewedPeds.length > 5) {
      recentlyViewedPeds.pop();
    }

    return {...state, recentlyViewedPeds};
  }),
);
