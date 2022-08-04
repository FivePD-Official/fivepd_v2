import { IUser } from '@interfaces';

export interface UserState {
  isSidebarCollapsed: boolean,
  isPanicModeActive: boolean,
  details: IUser,
}

export const userState = {
  state: (): UserState => {
    return {
      isSidebarCollapsed: false,
      isPanicModeActive: false,
      details: {} as IUser,
    }
  },
  mutations: {
    toggleSidebarCollapse(state: UserState) {
      state.isSidebarCollapsed = !state.isSidebarCollapsed;
    },
    togglePanicMode(state: UserState) {
      state.isPanicModeActive = !state.isPanicModeActive;
    },
    setUserDetails(state: UserState, details: IUser) {
      state.details = details;
    }
  },
  actions: {}
};