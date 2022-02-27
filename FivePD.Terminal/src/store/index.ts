import { createStore } from 'vuex';
import { userState } from './userState';

export const store = createStore({
  modules: {
    user: userState
  }
});