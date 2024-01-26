import { Page, PedHistory } from '@models';
import { createReducer, on } from '@ngrx/store';
import {
  AppActions,
} from '@store/app.actions';
import { Routes } from "../models/sidebar";

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
  previousMdtPath: ['mdt', 'dashboard'],
  devTools: devToolsDefaultState,
  dispatchMode: false,
  dispatchModeMapZoom: 3,
  windowMaximized: false,
  recentlyViewedPeds: [],
};

export const appReducer = createReducer(
  initialState,
  on(AppActions.setMDTPath, (state, {path}) => ({...state, previousMdtPath: path})),
  on(AppActions.addDevToolsCoordinate, (state, {coordinate}) => {
    const _state: AppStateModel = {
      ...state,
      devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}
    };
    const coordinates = [...state.devTools.coordinates.Items, coordinate];
    _state.devTools.coordinates.Items = coordinates;
    _state.devTools.coordinates.PageSize = coordinates.length;
    return _state;
  }),
  on(AppActions.clearDevToolsCoordinates, (state) => {
    const _state: AppStateModel = {
      ...state,
      devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}
    };
    _state.devTools.coordinates.Items = [];
    _state.devTools.coordinates.PageSize = 0;
    return _state;
  }),
  on(AppActions.toggleDispatchMode, (state, action) => ({...state, dispatchMode: action.state})),
  on(AppActions.dispatchModeMapZoom, (state, {zoom}) => ({...state, dispatchModeMapZoom: zoom})),
  on(AppActions.setWindowMaximized, (state, {maximized}) => ({...state, windowMaximized: maximized})),
  on(AppActions.addPedToRecentlyViewed, (state, {ped}) => {
    if (state.recentlyViewedPeds.some(x => x.Id === ped.Id)) {
      return state;
    }

    const recentlyViewedPeds = [ped, ...state.recentlyViewedPeds];
    if (recentlyViewedPeds.length > 5) {
      recentlyViewedPeds.pop();
    }

    return {...state, recentlyViewedPeds};
  }),
);
