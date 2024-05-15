import { Component, OnInit } from '@angular/core';
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
import { ModalService } from "@services/modals/modal/modal.service";
import { UserService } from "@services/user/user.service";
import { AudioService } from "@services/audio/audio.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent implements OnInit {

  calculatedContainerMargin: string = '';

  constructor(
    private readonly _window: WindowService, // do not remove OR refactor so event used in it are subscribed here
    private readonly _event: EventService,
    private readonly _localization: LocalizationService,
    private readonly _router: Router,
    private readonly _store: Store<GlobalState>,
    private readonly _menu: MenuService,
    private readonly _configService: ConfigService,
    private readonly _modalService: ModalService,
    private readonly _userService: UserService,
    private readonly _soundService: AudioService,
  ) {
  }

  ngOnInit(): void {
    this.onLoad();
    this._router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      filter(event => event.url !== '/' && event.url.startsWith('/mdt'))
    ).subscribe(event => {
      this._store.dispatch(AppActions.setMDTPath({path: event.url.split('/')}));
    });
    window.addEventListener('message', event => this._event.next(event.data));

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

    this._soundService.listenToEvents();
  }

  disableAnimations() {
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

  onLoad(): void {
    document.body.style.display = 'none';

    const style = document.createElement('style');
    style.appendChild(document.createTextNode(`\
      @font-face {
        font-family: 'Satoshi';
        src: url('../assets/Satoshi-Variable.woff2') format('woff2');
        font-weight: 300 900;
        font-display: block;
        font-style: normal;
      }
    `));
    document.head.appendChild(style);

    this._localization.load();
    this._menu.load();

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
  }
}
