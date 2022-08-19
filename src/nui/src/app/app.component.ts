import { Component, OnInit } from '@angular/core';
import { EventService } from '@services/event.service';
import { NuiEventType } from '@enums';
import { Localization } from '@models';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { SetLocalization } from '@store/app.actions';
import { HttpClient } from '@angular/common/http';
import { WindowService } from '@services/window.service';
import { LocalizationService } from '@services/localization.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent implements OnInit {

  @Dispatch() setLocalization = (localization: Localization) => new SetLocalization(localization);

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _window: WindowService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    this.onLoad();
    window.addEventListener('message', event => this._event.next(event.data));
    window.addEventListener('keydown', event => {
      if (event.code === 'Escape') {
        this._window.hide();
      }
    });
  }

  public get NuiEventType() {
    return NuiEventType;
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

}
