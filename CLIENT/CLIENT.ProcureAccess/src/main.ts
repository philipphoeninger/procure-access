import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { environment } from '../src/environments/environment';

if (!environment.production) {
  import('./dev/dev-reload-health').then(m => m.startApiHealthWatcher());
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
