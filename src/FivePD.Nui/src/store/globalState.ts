import type { RadialMenu, Localization } from '@interfaces';
import type { Menu } from '@models';
import { defaultLocalization } from '@interfaces';
import sanitizeHtml from 'sanitize-html';

export interface IListItem {
  Key: number;
  Title: string;
}

export interface GlobalState {
  radialMenus: RadialMenu[],
  menus: Menu[],
  availableLocales: IListItem[],
  localization: Localization,
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
    async setLocalization(state: GlobalState, localization: Localization) {
      await sanitizeObject(localization)
      state.localization = localization as any;
    },
  },
  actions: {}
};

function sanitizeObject(object: any): Promise<void> {
  return new Promise(resolve => {
    Object.keys(object).forEach(key => {
      if (object[key] && typeof object[key] === 'object') {
        sanitizeObject(object[key]);
        return;
      }
      object[key] = sanitizeHtml(object[key]);
      resolve();
    });
  });
}