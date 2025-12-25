import { Component, inject } from '@angular/core';
import { GameNotification, GameNotificationOptions } from "../../models/game-notification";
import { TranslatePipe } from "../../pipes/translate.pipe";
import { LocalizationService } from "@services/localization/localization.service";
import { DomSanitizer } from "@angular/platform-browser";
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-game-notification-wrapper',
  imports: [
    TranslatePipe,
    AsyncPipe
  ],
  templateUrl: './game-notification-wrapper.component.html',
  styleUrl: './game-notification-wrapper.component.sass'
})
export class GameNotificationWrapperComponent {

  private readonly _localizationService = inject(LocalizationService);
  private readonly _domSanitizer = inject(DomSanitizer);
  private readonly _store = inject<Store<GlobalState>>(Store);

  protected notifications$: Observable<GameNotification[]> = this._store.select('app', 'notifications');

  getTitle(notification: GameNotificationOptions) {
    return this._localizationService.translate(notification.Title, {...notification.TitleParams}, false);
  }

  getMessage(notification: GameNotificationOptions) {
    const message = this._localizationService.translate(notification.Message, {...notification.MessageParams}, false);
    return this._domSanitizer.bypassSecurityTrustHtml(message);
  }
}
