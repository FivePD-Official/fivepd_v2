import { Page } from '@models';
import { createReducer, on } from '@ngrx/store';
import {
  addDevToolsCoordinate,
  clearDevToolsCoordinates,
  setMdtPath,
  toggleSidebarCollapse
} from '@store/app.actions';

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

const initialState: AppStateModel = {
  isSidebarCollapsed: false,
  previousMdtPath: ['mdt', 'dashboard'],
  devTools: devToolsDefaultState,
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
    const _state: AppStateModel = {...state, devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}};
    const coordinates = [...state.devTools.coordinates.Items, action.payload];
    _state.devTools.coordinates.Items = coordinates;
    _state.devTools.coordinates.PageSize = coordinates.length;
    return _state;
  }),
  on(clearDevToolsCoordinates, (state) => {
    const _state: AppStateModel = {...state, devTools: {...state.devTools, coordinates: {...state.devTools.coordinates}}};
    _state.devTools.coordinates.Items = [];
    _state.devTools.coordinates.PageSize = 0;
    return _state;
  }),
);
