import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EventService } from '@services/event/event.service';
import { BaseNuiEvent, MenuShowEvent, TimerBarEvent } from '@models';
import { Router } from '@angular/router';
import { UserService } from '@services/user/user.service';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { BehaviorSubject } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class WindowService {

  visible: boolean = false; // TODO: this is only needed because of the close event spam
  previousMdtPath: string[] = ['mdt', 'dashboard'];

  visibleMdt$ = new BehaviorSubject(false);
  showInformation = new BehaviorSubject(false);
  showWelcomeModal = new BehaviorSubject(false);
  showMenu = new BehaviorSubject(false);

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _router: Router,
    private _userService: UserService,
    private readonly store: Store<GlobalState>,
  ) {
    this._event.onEvent<TimerBarEvent>('webpackInvalid').subscribe(() => this.close());
    this._event.onEvent<TimerBarEvent>('webpackOk').subscribe(() => this.close());
    window.addEventListener('keydown', event => {
      if (event.code === 'Escape') {
        this.close();
      }
    });

    this._event.onEvent<BaseNuiEvent>('fivepd:ui:close').subscribe(() => this.close()) // TODO: investigate why this subscription spams after the first close event
    this._event.onEvent<BaseNuiEvent>('fivepd:ui:view:mdt').subscribe(() => {
      this._userService.refreshData();
      this.onMdtOpen();
    });
    this._event.onEvent<BaseNuiEvent>('fivepd:ui:view:information').subscribe(() => {
      this.onInformationOpen();
    });
    this._event.onEvent<BaseNuiEvent>('fivepd:ui:view:welcomeModal').subscribe(() => {
      this.onWelcomeModalOpen();
    });
    this._event.onEvent<MenuShowEvent>('fivepd:ui:view:menu').subscribe(() => this.onMenuOpen());
    this._event.onEvent<MenuShowEvent>('fivepd:ui:view:radialMenu').subscribe(res => this.onRadialMenuOpen(res));
    this._event.onEvent<TimerBarEvent>('fivepd:ui:view:timer_bar').subscribe(res => this.onTimerBarEvent(res));

    this.store.subscribe(state => {
      this.previousMdtPath = state.app.previousMdtPath;
    });
  }

  show(): void {
    this.visible = true;
    this.visibleMdt$.next(true);
    document.body.style.display = 'block';
  }

  close(): void {
    if (this.showInformation.value || this.showWelcomeModal.value || this.showMenu.value) {
      this.triggerCloseEvent();
      this._router.navigate(['not-mdt']);
    } else {
      this.closeMDT();
    }
  }

  triggerCloseEvent(): void {
    this._http.post('fivepd:ui:close', {}).subscribe();
  }

  closeMDT(): void {
    this._router.navigate(['']);
    this.visibleMdt$.next(false);
    this.triggerCloseEvent();
  }

  private onMdtOpen(): void {
    this.show();
    this._router.navigate([...this.previousMdtPath]);
  }

  private onInformationOpen(): void {
    this.show();
    this.showInformation.next(!this.showInformation.value);
    this._router.navigate(['not-mdt']);
  }

  private onWelcomeModalOpen(): void {
    this.show();
    this.showWelcomeModal.next(!this.showWelcomeModal.value);
    this._router.navigate(['not-mdt']);
  }

  private onMenuOpen(): void {
    this.show();
    this.showMenu.next(!this.showMenu.value);
    this._router.navigate(['not-mdt']);
  }

  private onRadialMenuOpen(res: MenuShowEvent): void {
    this.show();
    this._router.navigate(['radial-menu', res.MenuId]);
  }

  private onTimerBarEvent(res: TimerBarEvent): void {
    if (res.Show) {
      this.show();
      this._router.navigate(['timer-bar']);
    } else {
      this.triggerCloseEvent();
    }
  }
}
