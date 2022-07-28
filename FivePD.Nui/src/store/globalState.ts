import { IMenu, ILocalization, defaultLocalization, IListItem } from "@interfaces";
import { Menu } from "@utils";

export interface GlobalState {
  radialMenus: IMenu[],
  menus: Menu[],
  availableLocales: IListItem[],
  localization: ILocalization,
}

export const globalState = {
  state: (): GlobalState => {
    return {
      radialMenus: [],
      menus: [],
      availableLocales: [],
      localization: defaultLocalization,
    }
  },
  mutations: {
    setRadialMenus(state: GlobalState, radialMenus: IMenu[]) {
      state.radialMenus = radialMenus;
    },
    setMenus(state: GlobalState, menus: Menu[]) {
      state.menus = menus;
    },
    setAvailableLocales(state: GlobalState, availableLocales: IListItem[]) {
      state.availableLocales = availableLocales;
    },
    setLocalization(state: GlobalState, localization: ILocalization) {
      state.localization = localization;
    },
  },
  actions: {}
};