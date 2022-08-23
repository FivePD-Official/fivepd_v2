import { Component, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { NuiEventType } from '@enums';
import { Localization } from '@models';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { SetLocalization, SetMdtPath } from '@store/app.actions';
import { HttpClient } from '@angular/common/http';
import { WindowService } from '@services/window/window.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent implements OnInit {

  @Dispatch() setLocalization = (localization: Localization) => new SetLocalization(localization);
  @Dispatch() setMdtPath = (path: string[]) => new SetMdtPath(path);

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _window: WindowService,
    private _localization: LocalizationService,
    private _router: Router,
  ) {}

  ngOnInit(): void {
    this.onLoad();
    this._router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      filter(event => event.url !== '/')
    ).subscribe(event => {
      this.setMdtPath(event.url.split('/'));
    });
    window.addEventListener('message', event => this._event.next(event.data));
    window.addEventListener('keydown', event => {
      if (event.code === 'Escape') {
        this._window.hide();
      }
    });
  }

  onLoad(): void {
    window.addEventListener('load', () => {

      document.head.innerHTML = document.head.innerHTML + '<base href="/Libraries/FivePD.Terminal/"/>';
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

      this.sendAvailabilityToClient();
      this._localization.load();
    });
  }

  sendAvailabilityToClient(): void {
    this._http.post(NuiEventType.IsNuiReady, {}).subscribe();
  }

  setPathInState(path: string): void {
    this.setMdtPath(path.split('/'));
  }

}
