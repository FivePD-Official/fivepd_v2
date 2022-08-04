import type { Notification } from '@interfaces';
import { Subject, Observable } from 'rxjs';

const notificationHandler = new Subject<Notification>();

export function showNotification(notification: Notification) {
  notificationHandler.next(notification);
}

export function onNotification(): Observable<Notification> {
  return notificationHandler.pipe();
}
