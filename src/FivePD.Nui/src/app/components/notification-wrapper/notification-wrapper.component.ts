import { Component, OnDestroy, OnInit } from '@angular/core';
import { NotificationService } from '@services/notification/notification.service';
import { Subscription } from 'rxjs';
import { Notification } from '@models';
import { NotificationComponent } from '../notification/notification.component';
import { NgFor } from '@angular/common';

type NotificationWithTimer = Notification & { timer: number; exit: boolean };

@Component({
    selector: 'app-notification-wrapper',
    templateUrl: './notification-wrapper.component.html',
    styleUrls: ['./notification-wrapper.component.sass'],
    imports: [NgFor, NotificationComponent]
})
export class NotificationWrapperComponent implements OnInit, OnDestroy {

  intervalId?: number;
  notifications: NotificationWithTimer[] = [];
  notifications$?: Subscription;

  constructor(
    private _notification: NotificationService,
  ) {
  }

  ngOnInit(): void {
    this.notifications$ = this._notification.onShow().subscribe((notification) => {
      this.notifications.push({
        ...notification,
        timer: 0,
        exit: false,
      });
    });

    this.intervalId = setInterval(() => {
      this.notifications.forEach((notification) => {
        if (notification.timer === 4) {
          notification.exit = true;
          notification.timer++;
        } else {
          notification.timer++;
        }
      });
      this.notifications = this.notifications.filter((x) => x.timer !== 5);
    }, 1000) as unknown as number;
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
    this.notifications = [];
    this.notifications$?.unsubscribe();
  }
}
