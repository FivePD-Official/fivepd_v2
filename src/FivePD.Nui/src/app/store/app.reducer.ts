import { Page, PedData, Vector3, VehicleData } from '@models';
import { createReducer, on } from '@ngrx/store';
import {
  AppActions,
} from '@store/app.actions';
import { GameNotificationOptions } from '../models/game-notification';

export interface WithDate {
  date: Date;
}

export type CoordinateWithDate = Vector3 & WithDate;

export interface DevToolsState {
  coordinates: Page<CoordinateWithDate>;
}

export interface GameNotification {
  notification: GameNotificationOptions;
  notificationKeysToDisplay: { key: string; value: string }[];
  isExiting: boolean;
  remainingTime: number;
}

export interface AppStateModel {
  previousMdtPath: string[];
  devTools: DevToolsState;
  dispatchMode: boolean;
  dispatchModeMapZoom: number;
  windowMaximized: boolean;
  recentlyViewedPeds: PedData[];
  recentlyViewedVehicles: VehicleData[];
  notifications: GameNotification[];
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
  recentlyViewedVehicles: [],
  notifications: [],
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
  on(AppActions.addVehicleToRecentlyViewed, (state, {vehicle}) => {
    if (state.recentlyViewedVehicles.some(x => x.Id === vehicle.Id)) {
      return state;
    }

    const recentlyViewedVehicles = [vehicle, ...state.recentlyViewedVehicles];
    if (recentlyViewedVehicles.length > 5) {
      recentlyViewedVehicles.pop();
    }

    return {...state, recentlyViewedVehicles};
  }),
  on(AppActions.addNotification, (state, {notification}) => ({
    ...state,
    notifications: [...state.notifications, notification]
  })),
  on(AppActions.removeNotification, (state, {notification}) => ({
    ...state,
    notifications: state.notifications.filter(n => n.notification !== notification)
  })),
  on(AppActions.updateNotification, (state, {notification, changes}) => ({
    ...state,
    notifications: state.notifications.map(n => n.notification === notification ? {...n, ...changes} : n)
  })),
);
