import { IListItem } from "@interfaces";

export interface UserState {
  isSidebarCollapsed: boolean,
  isPanicModeActive: boolean,
  availableLocales: IListItem[],
  localization: any,
}

export const userState = {
  state: (): UserState => {
    return {
      isSidebarCollapsed: false,
      isPanicModeActive: false,
      availableLocales: [],
      localization: {}
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
    setLocalization(state: UserState, localization: any) {
      state.localization = localization;
    }
  },
  actions: {}
};