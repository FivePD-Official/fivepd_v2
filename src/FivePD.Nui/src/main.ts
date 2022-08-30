import { createApp } from 'vue';
import App from './App.vue';
import { createRouter, createWebHistory } from 'vue-router';
import routes from './routes';
import directives from './directives';
import { store } from '@store';
import {post} from '@utils';
import {NuiEventType} from '@enums';
import {defer, from, map, retry} from 'rxjs';

// This is the main entry point for the NUI.
// It checks every second to see if the client is ready.
// If it is, it starts the app.
// If it isn't, it waits another second and tries again.
defer(() => from(post(NuiEventType.ClientReady)))
  .pipe(
    map((val: any) => {
      if (val === true) {
        return val;
      } else {
        throw new Error('Client not ready');
      }
    }),
    retry({
      delay: 1000,
      count: 15
    })
  )
  .subscribe({
    next: value => startApp(),
    error: err => console.error('[FivePD ERROR] Error starting NUI. Max retries reached.')
  });

function startApp()
{
  const router = createRouter({
    history: createWebHistory(),
    routes
  });

  const app = createApp(App);

  for (const item of Object.entries(directives)) {
    app.directive(item[0], item[1]);
  }

  app
    .use(router)
    .use(store)
    .mount('#app');
}