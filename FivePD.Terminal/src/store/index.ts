import { createStore } from 'vuex';
import { userState } from './userState';
import { menuState } from './menuState';

export const store = createStore({
  modules: {
    user: userState,
    menu: menuState
  }
});