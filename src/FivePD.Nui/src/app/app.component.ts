import { Component, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { NuiEventType, SetId } from '@enums';
import { Localization } from '@models';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { SetLocalization, SetMdtPath } from '@store/app.actions';
import { HttpClient } from '@angular/common/http';
import { WindowService } from '@services/window/window.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { MenuService } from '@services/menu/menu.service';

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
    private _looseData: LooseDataService,
    private _menu: MenuService,
  ) {}

  ngOnInit(): void {
    this.onLoad();
    this._router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      filter(event => event.url !== '/' && event.url.startsWith('/mdt'))
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
    // TODO: Not working, change this in the index.html with the pipeline
    // document.head.innerHTML = document.head.innerHTML + '<base href="/Libraries/FivePD.Nui/"/>';
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
  }

}
