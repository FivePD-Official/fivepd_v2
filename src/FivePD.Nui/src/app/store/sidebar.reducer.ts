import { createReducer, on } from '@ngrx/store';
import { Routes } from "../models/sidebar";
import { SidebarActions } from "@store/sidebar.actions";

export interface SidebarStateModel {
  routes: Routes;
  collapsed: boolean;
}

const initialState: SidebarStateModel = {
  routes: [],
  collapsed: false,
};

export const sidebarReducer = createReducer(
  initialState,
  on(SidebarActions.setRoutes, (state, {routes}) => {
    if (state.routes.length) {
      return state;
    }

    return {...state, routes};
  }),
  on(SidebarActions.toggleRoute, (state, {route}) => {
    const routeIndex = state.routes.findIndex(r => r === route);
    if (routeIndex > -1) {
      const updatedRoutes = [...state.routes];
      updatedRoutes[routeIndex] = { ...route, open: !route.open };

      return {
        ...state,
        routes: updatedRoutes,
      };
    }

    return state;
  }),
  on(SidebarActions.toggleCollapse, (state) => ({...state, collapsed: !state.collapsed})),
);
