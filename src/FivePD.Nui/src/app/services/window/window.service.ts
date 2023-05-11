import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NuiEventType } from '@enums';
import { EventService } from '@services/event/event.service';
import { BaseNuiEvent, MenuShowEvent, TimerBarEvent } from '@models';
import { Observable, Subject, takeUntil } from 'rxjs';
import { Router } from '@angular/router';
import { UserService } from '@services/user/user.service';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';

@Injectable({
  providedIn: 'root'
})
export class WindowService {

  visible: boolean = false; // TODO: this is only needed because of the close event spam
  previousMdtPath: string[] = ['mdt', 'dashboard'];

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _router: Router,
    private _userService: UserService,
    private readonly store: Store<GlobalState>,
  ) {
    this._event.onEvent<TimerBarEvent>('webpackInvalid' as NuiEventType).subscribe(() => this.hide());
    this._event.onEvent<TimerBarEvent>('webpackOk' as NuiEventType).subscribe(() => this.hide());

    this._event.onEvent<BaseNuiEvent>(NuiEventType.Close).subscribe(() => this.hide()) // TODO: investigate why this subscription spams after the first close event
    this._event.onEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(() => {
      this._userService.refreshData();
      this.onMdtOpen();
    });
    this._event.onEvent<MenuShowEvent>(NuiEventType.MenuView).subscribe(res => this.onMenuOpen(res));
    this._event.onEvent<MenuShowEvent>(NuiEventType.RadialMenu).subscribe(res => this.onRadialMenuOpen(res));
    this._event.onEvent<TimerBarEvent>(NuiEventType.TimerBar).subscribe(res => this.onTimerBarEvent(res));

    this.store.subscribe(state => {
      this.previousMdtPath = state.app.previousMdtPath;
    });
  }

  show(): void {
    this.visible = true;
    document.body.style.display = 'block';
  }

  hide(): void {
    if (!this.visible) {
      return;
    }

    this.visible = false;
    document.body.style.display = 'none';
    this._http.post(NuiEventType.Close, {}).subscribe();
  }

  private onMdtOpen(): void {
    this.show();
    this._router.navigate([...this.previousMdtPath]);
  }

  private onMenuOpen(res: MenuShowEvent): void {
    this.show();
    this._router.navigate(['menu', res.menuId]);
    console.log('navigated to menu')
  }

  private onRadialMenuOpen(res: MenuShowEvent): void {
    this.show();
    this._router.navigate(['radial-menu', res.menuId]);
  }

  private onTimerBarEvent(res: TimerBarEvent): void {
    if (res.show) {
      this.show();
      this._router.navigate(['timer-bar']);
    } else {
      this.hide();
    }
  }
}
