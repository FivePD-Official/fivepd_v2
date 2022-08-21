import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { Notification } from '@models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  private _notification$ = new Subject<Notification>();

  constructor() { }

  show(notification: Notification): void {
    this._notification$.next(notification);
  }

  onShow(): Observable<Notification> {
    return this._notification$.asObservable();
  }
}
