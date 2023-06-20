import { Component, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { HttpClient } from '@angular/common/http';
import { WindowService } from '@services/window/window.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { setMdtPath } from '@store/app.actions';
import { MenuService } from '@services/menu/menu.service';
import { ConfigService } from "@services/config/config.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent implements OnInit {

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _window: WindowService,
    private _localization: LocalizationService,
    private _router: Router,
    private _looseData: LooseDataService,
    private readonly store: Store<GlobalState>,
    private _menu: MenuService,
    private readonly _configService: ConfigService,
  ) {}

  ngOnInit(): void {
    this.onLoad();
    this._router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      filter(event => event.url !== '/' && event.url.startsWith('/mdt'))
    ).subscribe(event => {
      this.store.dispatch(setMdtPath({payload: event.url.split('/')}));
    });
    window.addEventListener('message', event => this._event.next(event.data));
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

      mainContainer.style.margin = `${tbMargin}px ${lrMargin}px`;
    });
  }
}
