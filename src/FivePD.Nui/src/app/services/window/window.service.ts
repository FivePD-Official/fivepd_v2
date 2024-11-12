import { inject, Injectable } from '@angular/core';
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

  private readonly _httpClient = inject(HttpClient);
  private readonly _eventService = inject(EventService);
  private readonly _router = inject(Router);
  private readonly _userService = inject(UserService);
  private readonly _store = inject<Store<GlobalState>>(Store);

  private _listening = false;
  private previousMdtPath: string[] = ['mdt', 'dashboard'];

  readonly visibleMdt$ = new BehaviorSubject(false);
  readonly showInformation = new BehaviorSubject(false);
  readonly showWelcomeModal = new BehaviorSubject(false);
  readonly showMenu = new BehaviorSubject(false);

  listenToEvents(): void {
    if (this._listening) {
      return;
    }

    this._listening = true;

    this._eventService.onEvent<TimerBarEvent>('webpackInvalid').subscribe(() => this.close());
    this._eventService.onEvent<TimerBarEvent>('webpackOk').subscribe(() => this.close());
    window.addEventListener('keydown', event => {
      if (event.code === 'Escape') {
        this.close();
      }
    });

    this._eventService.onEvent<BaseNuiEvent>('fivepd:ui:close').subscribe(() => this.close());
    this._eventService.onEvent<BaseNuiEvent>('fivepd:ui:view:mdt').subscribe(() => {
      this._userService.refreshData();
      this.onMdtOpen();
    });
    this._eventService.onEvent<BaseNuiEvent>('fivepd:ui:view:information').subscribe(() => {
      this.onInformationOpen();
    });
    this._eventService.onEvent<BaseNuiEvent>('fivepd:ui:view:welcomeModal').subscribe(() => {
      this.onWelcomeModalOpen();
    });
    this._eventService.onEvent<MenuShowEvent>('fivepd:ui:view:menu').subscribe(() => this.onMenuOpen());
    this._eventService.onEvent<MenuShowEvent>('fivepd:ui:view:radialMenu').subscribe(res => this.onRadialMenuOpen(res));
    this._eventService.onEvent<TimerBarEvent>('fivepd:ui:view:timer_bar').subscribe(res => this.onTimerBarEvent(res));

    this._store.subscribe(state => {
      this.previousMdtPath = state.app.previousMdtPath;
    });
  }

  show(): void {
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
    this._httpClient.post('fivepd:ui:close', {}).subscribe();
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
