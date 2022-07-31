import type { RadialMenu, ILocalization } from "@interfaces";
import type { Menu } from "@models";
import { defaultLocalization } from "@interfaces";

export interface IListItem {
  Key: number;
  Title: string;
}

export interface GlobalState {
  radialMenus: RadialMenu[],
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
    setRadialMenus(state: GlobalState, radialMenus: RadialMenu[]) {
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