import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ClientEvents, NuiEventType } from '@enums';
import { EventService } from './event.service';
import { BaseNuiEvent, IUser, MenuShowEvent } from '@interfaces';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { SetUser } from '../store/user.actions';
import { Select } from '@ngxs/store';
import { AppState } from '../store/app.state';

@Injectable({
  providedIn: 'root'
})
export class WindowService {

  @Select(AppState.previousMdtPath) previousMdtPath$!: Observable<string[]>;
  @Dispatch() setUser = (user: IUser) => new SetUser(user);

  visible: boolean = false; // TODO: this is only needed because of the close event spam
  previousMdtPath: string[] = ['mdt', 'dashboard'];

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _router: Router,
  ) {
    this._event.onEvent<BaseNuiEvent>(NuiEventType.Close).subscribe(() => this.onClose()) // TODO: investigate why this subscription spams after the first close event
    this._event.onEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(() => this.onMdtOpen());
    this._event.onEvent<MenuShowEvent>(NuiEventType.MenuView).subscribe(res => this.onMenuOpen(res));
    // this._event.onEvent<MenuShowEvent>(NuiEventType.RadialMenu).subscribe(res => this.onMenuOpen(res));

    this.previousMdtPath$.subscribe(value => this.previousMdtPath = value);
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

  private onClose(): void {
    this.hide();
    this._router.navigate(['/']);
  }

  private onMdtOpen(): void {
    this.show();
    this._router.navigate([...this.previousMdtPath]);
    this._http.post<IUser>(ClientEvents.GetPlayerDetails, {}).subscribe(res => this.setUser(res));
  }

  private onMenuOpen(res: MenuShowEvent): void {
    this.show();
    this._router.navigate(['menu', res.menuId]);
  }
}
