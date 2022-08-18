import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor, HttpResponse
} from '@angular/common/http';
import { map, Observable } from 'rxjs';

@Injectable()
export class CustomHttpInterceptor implements HttpInterceptor {

  constructor() {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const apiReq = request.clone({
      url: `https://fivepd/${request.url}`
    });

    return next.handle(apiReq).pipe(
      map(event => {
        if (event instanceof HttpResponse) {

          let body;
          if (typeof event.body === 'string') {
            try {
              body = JSON.parse(event.body);
            } catch(_) {
              body = event.body;
            }
          }

          event = event.clone({
            body
          });
        }
        return event;
      })
    );

    return next.handle(apiReq);
  }
}
