import { Component, inject, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { WindowService } from '@services/window/window.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { AppActions } from '@store/app.actions';
import { MenuService } from '@services/menu/menu.service';
import { ConfigService } from "@services/config/config.service";
import { AudioService } from "@services/audio/audio.service";
import { FeedbackService } from "@services/feedback/feedback.service";
import { BaseNuiEvent } from "@models";
import { UserService } from "@services/user/user.service";
import { environment } from "../environments/environment";

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.sass'],
    standalone: false
})
export class AppComponent implements OnInit {

  protected readonly _windowService = inject(WindowService);
  private readonly _eventService = inject(EventService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _router = inject(Router);
  private readonly _store = inject<Store<GlobalState>>(Store);
  private readonly _menuService = inject(MenuService);
  private readonly _configService = inject(ConfigService);
  private readonly _soundService = inject(AudioService);
  private readonly _feedbackService = inject(FeedbackService);
  private readonly _userService = inject(UserService);

  private calculatedContainerMargin: string = '';

  ngOnInit(): void {
    this.setFont();
    this._menuService.load();
    this._localizationService.load();
    this._windowService.listenToEvents();
    this._soundService.listenToEvents();

    this._router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      filter(event => event.url !== '/' && event.url.startsWith('/mdt'))
    ).subscribe(event => {
      this._store.dispatch(AppActions.setMDTPath({path: event.url.split('/')}));
    });

    window.addEventListener('message', event => this._eventService.next(event.data));

    this._eventService.onEvent('fivepd:lifecycle:serverBootstrapped').subscribe(() => {
      this._menuService.load();
      this._userService.refreshData();
    });

    this._configService.getSafeZoneSize().subscribe(safeZoneSize => {
      const safeZone = safeZoneSize * 0.5;
      const tbMargin = window.screen.height * safeZone;
      const lrMargin = window.screen.width * safeZone;
      const mainContainer = document.querySelector('.main-container') as HTMLDivElement;
      if (!mainContainer) {
        return;
      }

      this.calculatedContainerMargin = `${tbMargin}px ${lrMargin}px`;
      mainContainer.style.inset = this.calculatedContainerMargin;
    });

    this._store.select('app', 'windowMaximized').subscribe(windowMaximized => {
      const mainContainer = document.querySelector('.main-container') as HTMLDivElement;
      if (!mainContainer) {
        return;
      }

      const enableAnimations = this.disableAnimations();

      if (windowMaximized) {
        mainContainer.style.inset = '0';
      } else {
        mainContainer.style.inset = this.calculatedContainerMargin;
      }

      enableAnimations();
    });

    this._feedbackService.collectErrors();
  }

  private disableAnimations() {
    const css = document.createElement('style');
    css.appendChild(
      document.createTextNode(`*{-webkit-transition:none!important;-moz-transition:none!important;-o-transition:none!important;-ms-transition:none!important;transition:none!important}`)
    );
    document.head.appendChild(css);

    return () => {
      ;(() => window.getComputedStyle(document.body))();

      setTimeout(() => {
        document.head.removeChild(css);
      }, 1);
    }
  }

  private setFont(): void {
    document.body.style.display = 'none';

    const style = document.createElement('style');
    if (environment.production) {
      style.appendChild(document.createTextNode(`\
        @font-face {
          font-family: 'Satoshi';
          src: url('assets/Satoshi-Variable.woff2') format('woff2');
          font-weight: 300 900;
          font-display: block;
          font-style: normal;
        }
      `));
    } else {
      style.appendChild(document.createTextNode(`\
        @font-face {
          font-family: 'Satoshi';
          src: url('../assets/Satoshi-Variable.woff2') format('woff2');
          font-weight: 300 900;
          font-display: block;
          font-style: normal;
        }
      `));
    }
    document.head.appendChild(style);
  }
}
