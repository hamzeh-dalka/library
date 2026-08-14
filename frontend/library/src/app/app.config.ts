import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';
import { routes } from './app.routes';
import { provideHttpClient , withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(
      // Order matters: authInterceptor attaches the token on the way out,
      // errorInterceptor reacts to 401s on the way back.
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    provideRouter(routes)
  ]
};
