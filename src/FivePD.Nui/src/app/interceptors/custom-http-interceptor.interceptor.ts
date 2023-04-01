import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { NotificationService } from '@services/notification/notification.service';
import { LocalizationService } from '@services/localization/localization.service';
import { ActionResponse } from '@models';

@Injectable()
export class CustomHttpInterceptor implements HttpInterceptor {

  constructor(
    private _notification: NotificationService,
    private _localization: LocalizationService,
  ) {}

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
      }),
      tap(event => {
        if (event instanceof HttpResponse) {
          const body = event.body as ActionResponse;
          if (body.hasOwnProperty('Title') && body.hasOwnProperty('Description') &&
            body.hasOwnProperty('Status') && body.hasOwnProperty('Params')
          ) {
            this._notification.show({
              type: body.Status ? 'success' : 'error',
              title: this._localization.translate(body.Title),
              description: this._localization.translate(body.Description, body.Params),
            });
          }
        }
      }),
    );
  }
}
