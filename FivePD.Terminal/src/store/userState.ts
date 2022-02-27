import { themes } from '../utils';

export interface UserState {
  isSidebarCollapsed: boolean,
  isPanicModeActive: boolean,
  isDarkThemeActive: boolean
}

export const userState = {
  state: (): UserState => {
    return {
      isSidebarCollapsed: false,
      isPanicModeActive: false,
      isDarkThemeActive: false
    }
  },
  mutations: {
    toggleSidebarCollapse(state: UserState) {
      state.isSidebarCollapsed = !state.isSidebarCollapsed;
    },
    togglePanicMode(state: UserState) {
      state.isPanicModeActive = !state.isPanicModeActive;
    },
    toggleDarkTheme(state: UserState) {
      state.isDarkThemeActive = !state.isDarkThemeActive;
    }
  },
  actions: {
    toggleDarkTheme(ctx: any) {
      ctx.commit('toggleDarkTheme');

      const theme = ctx.state.isDarkThemeActive ? themes.light : themes.dark;
      for (const [key, value] of Object.entries(theme)) {
        document.documentElement.style.setProperty(key, value);
      }

    }
  }
};