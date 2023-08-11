import { Component, OnInit } from '@angular/core';
import { User } from '@models';
import { WindowService } from '@services/window/window.service';
import { GlobalState } from '@store';
import { Store } from '@ngrx/store';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { disableDispatchMode, setWindowMaximized } from "@store/app.actions";
import { Router } from "@angular/router";
import { DispatchService } from "@services/dispatch/dispatch.service";
import { Browser } from "leaflet";
import win = Browser.win;

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.sass']
})
export class HeaderComponent implements OnInit {

  user?: User;
  dispatchMode: boolean = false;
  windowMaximized: boolean = false;

  constructor(
    private readonly _store: Store<GlobalState>,
    private readonly _dispatchService: DispatchService,
    public readonly window: WindowService,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.user = state.user.details.User;
      this.dispatchMode = state.app.dispatchMode;
      this.windowMaximized = state.app.windowMaximized;
    });
  }

  ngOnInit(): void {
  }

  disableDispatchMode(): void {
    this._dispatchService.disable();
  }

  toggleMaximizedWindow(): void {
    this._store.dispatch(setWindowMaximized({payload: !this.windowMaximized}));
  }

  protected readonly Browser = Browser;
  protected readonly win = win;
}
