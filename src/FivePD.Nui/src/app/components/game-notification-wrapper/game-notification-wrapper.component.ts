import { Component, inject, OnInit } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { GameNotificationOptions } from "../../models/game-notification";
import { delay, mergeMap, of, switchMap, takeUntil, tap, timer } from "rxjs";
import { TranslatePipe } from "../../pipes/translate.pipe";
import { AudioService } from "@services/audio/audio.service";
import { LocalizationService } from "@services/localization/localization.service";
import { DomSanitizer } from "@angular/platform-browser";

interface GameNotification {
  notification: GameNotificationOptions;
  notificationKeysToDisplay: { key: string; value: string }[];
  isExiting: boolean;
  remainingTime: number;
}

@Component({
    selector: 'app-game-notification-wrapper',
    imports: [
        TranslatePipe
    ],
    templateUrl: './game-notification-wrapper.component.html',
    styleUrl: './game-notification-wrapper.component.sass'
})
export class GameNotificationWrapperComponent implements OnInit {

  private readonly _eventService = inject(EventService);
  private readonly _audioService = inject(AudioService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _domSanitizer = inject(DomSanitizer);

  protected notifications: GameNotification[] = [];

  ngOnInit(): void {
    this._eventService.onEvent<GameNotificationOptions>('fivepd:gameNotification:notify')
      .pipe(
        mergeMap(notification => of(null).pipe(
          tap(() => this.addNotification(notification)),
          switchMap(() => this.startCountdown(notification)),
          delay(notification.Timeout),
          tap(() => this.startNotificationExit(notification)),
          delay(400),
          tap(() => this.notifications = this.notifications.filter(x => x.notification !== notification)),
        )),
      )
      .subscribe();
  }

  getTitle(notification: GameNotificationOptions) {
    const title = this._localizationService.translate(notification.Title, notification.TitleParams, false);
    return title;
  }

  getMessage(notification: GameNotificationOptions) {
    const message = this._localizationService.translate(notification.Message, notification.MessageParams, false);
    return this._domSanitizer.bypassSecurityTrustHtml(message);
  }

  private addNotification(notification: GameNotificationOptions) {
    this.notifications.push({
      notification,
      notificationKeysToDisplay: Object.entries(notification.Keys ?? {}).map(x => ({key: x[0], value: x[1]})),
      isExiting: false,
      remainingTime: notification.Timeout / 1000
    });

    if (notification.PlayAudio) {
      this._audioService.playSound(notification.PlayAudio);
    }
  }

  private startNotificationExit(notification: GameNotificationOptions) {
    const index = this.notifications.findIndex(n => n.notification === notification);
    if (index > -1) {
      this.notifications[index].isExiting = true;
    }
  }

  private startCountdown(notification: GameNotificationOptions) {
    const notificationWrapper = this.notifications.find(n => n.notification === notification);
    if (!notificationWrapper) {
      return of(null);
    }

    return timer(0, 1000).pipe(
      takeUntil(timer(notification.Timeout)),
      tap(() => {
        if (notificationWrapper.remainingTime > 0) {
          notificationWrapper.remainingTime--;
        }
      })
    );
  }
}
