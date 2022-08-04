import { createStore, useStore } from 'vuex';
import { userState, UserState } from './userState';
import { globalState, GlobalState } from './globalState';

export type State = {
  user: UserState,
  global: GlobalState,
};

export const store = createStore<State>({
  modules: {
    user: userState,
    global: globalState,
  },
  state: {} as State
});

export const useState = () => useStore<State>().state;