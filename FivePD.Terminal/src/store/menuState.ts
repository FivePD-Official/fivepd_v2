export interface MenuState {
  currentItemIndex: number,
  itemCount: number,
  selectedItem: boolean
}

export const menuState = {
  state: (): MenuState => {
    return {
      currentItemIndex: 0,
      itemCount: 0,
      selectedItem: false
    }
  },
  mutations: {
    setItemCount(state: MenuState, itemCount: number) {
      state.itemCount = itemCount;
    },
    moveUp(state: MenuState) {
      if (state.currentItemIndex > 0) state.currentItemIndex = state.currentItemIndex - 1;
      else state.currentItemIndex = state.itemCount - 1;
    },
    moveDown(state: MenuState) {
      if (state.currentItemIndex < state.itemCount-1) state.currentItemIndex = state.currentItemIndex + 1;
      else state.currentItemIndex = 0;
    },
    select(state: MenuState) {
      state.selectedItem = true;
      setTimeout(() => {
        state.selectedItem = false;
      }, 100);
    },
  }
};