import { Component, OnInit } from '@angular/core';
import { User } from '@models';
import { WindowService } from '@services/window/window.service';
import { GlobalState } from '@store';
import { Store } from '@ngrx/store';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { setWindowMaximized } from "@store/app.actions";
import { DispatchService } from "@services/dispatch/dispatch.service";
import { IconComponent } from '../icon/icon.component';
import { ButtonComponent } from '../button/button.component';
import { DatePipe, NgIf } from '@angular/common';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.sass'],
  standalone: true,
  imports: [NgIf, ButtonComponent, IconComponent, DatePipe]
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

  get dateNow() {
    return new Date();
  }

  disableDispatchMode(): void {
    this._dispatchService.disable();
  }

  toggleMaximizedWindow(): void {
    this._store.dispatch(setWindowMaximized({payload: !this.windowMaximized}));
  }
}
