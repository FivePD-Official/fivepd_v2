import { IMenu } from "@interfaces";

export interface GlobalState {
  radialMenus: IMenu[]
}

export const globalState = {
  state: (): GlobalState => {
    return {
      radialMenus: []
    }
  },
  mutations: {
    setRadialMenus(state: GlobalState, radialMenus: IMenu[]) {
      state.radialMenus = radialMenus;
    },
  },
  actions: {}
};