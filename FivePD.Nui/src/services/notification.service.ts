import type { Notification } from '@interfaces';
import { Observable, Subject } from 'rxjs';

class NotificationService {
  
  private _notification$ = new Subject<Notification>();
  
  show(notification: Notification): void {
    this._notification$.next(notification);
  }
  
  onShow(): Observable<Notification> {
    return this._notification$.asObservable();
  }
}

export const notificationService = new NotificationService();