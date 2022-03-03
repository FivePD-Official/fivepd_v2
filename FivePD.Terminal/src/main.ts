import { createApp } from 'vue';
import App from './App.vue';
import { createRouter, createWebHistory } from 'vue-router';
import routes from './routes';
import directives from './directives';
import { store } from './store';
import mitt from 'mitt';

const router = createRouter({
  history: createWebHistory(),
  routes
});

const app = createApp(App);
const emitter = mitt();

app.config.globalProperties.emitter = emitter;

for (const item of Object.entries(directives)) {
  app.directive(item[0], item[1]);
}

app
  .use(router)
  .use(store)
  .mount('#app');
