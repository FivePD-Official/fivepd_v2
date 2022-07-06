import { IListItem, ILocalization, IUser } from "@interfaces";

export interface UserState {
  isSidebarCollapsed: boolean,
  isPanicModeActive: boolean,
  availableLocales: IListItem[],
  localization: ILocalization,
  details: IUser,
}

export const userState = {
  state: (): UserState => {
    return {
      isSidebarCollapsed: false,
      isPanicModeActive: false,
      availableLocales: [],
      localization: {} as ILocalization,
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
    setAvailableLocales(state: UserState, availableLocales: IListItem[]) {
      state.availableLocales = availableLocales;
    },
    setLocalization(state: UserState, localization: ILocalization) {
      state.localization = localization;
    },
    setUserDetails(state: UserState, details: IUser) {
      state.details = details;
    }
  },
  actions: {}
};