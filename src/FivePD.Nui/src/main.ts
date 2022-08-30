import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { defer, map, retry } from 'rxjs';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { fromFetch } from 'rxjs/fetch';

if (environment.production) {
  enableProdMode();
}

defer(() => fromFetch('https://fivepd/ready'))
  .pipe(
    map(res => {
      if (res.status === 200) {
        return res;
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
    next: () => startApp(),
    error: () => console.error('[FivePD ERROR] Error starting NUI. Max retries reached.')
  });

function startApp() {
  platformBrowserDynamic().bootstrapModule(AppModule)
    .catch(err => console.error(err));
}
