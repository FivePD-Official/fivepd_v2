import { Component, inject, OnInit } from '@angular/core';
import { User } from '@models';
import { WindowService } from '@services/window/window.service';
import { GlobalState } from '@store';
import { Store } from '@ngrx/store';
import { DispatchService } from "@services/dispatch/dispatch.service";
import { IconComponent } from '../icon/icon.component';
import { ButtonComponent } from '../button/button.component';
import { DatePipe } from '@angular/common';
import { AppActions } from "@store/app.actions";
import { LocalizationService } from "@services/localization/localization.service";
import { injectDestroy } from "../../helpers/destroy-ref";
import { takeUntil } from "rxjs";
import { FeedbackService } from "@services/feedback/feedback.service";

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.sass'],
    imports: [ButtonComponent, IconComponent, DatePipe]
})
export class HeaderComponent implements OnInit {

  private readonly destroy$ = injectDestroy();
  private readonly _store = inject<Store<GlobalState>>(Store);
  private readonly _dispatchService = inject(DispatchService);
  private readonly _localizationService = inject(LocalizationService);
  protected readonly feedbackService = inject(FeedbackService);
  protected readonly window = inject(WindowService);

  user?: User;
  dispatchMode: boolean = false;
  windowMaximized: boolean = false;

  ngOnInit(): void {
    this._store.pipe(takeUntil(this.destroy$)).subscribe(state => {
      this.user = state.user.details.User;
      this.dispatchMode = state.app.dispatchMode;
      this.windowMaximized = state.app.windowMaximized;
    });
  }

  get loggedInAsText() {
    return this._localizationService.translate('UI.LABEL.HEADER.LOGGED_IN_AS', {
      name: this.user?.Name ?? '-',
    })
  }

  get dateNow() {
    return new Date();
  }

  disableDispatchMode(): void {
    this._dispatchService.disable();
  }

  toggleMaximizedWindow(): void {
    this._store.dispatch(AppActions.setWindowMaximized({maximized: !this.windowMaximized}));
  }
}
