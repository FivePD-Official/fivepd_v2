import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NuiEventType } from '@enums';
import { EventService } from '@services/event/event.service';
import { BaseNuiEvent, MenuShowEvent } from '@models';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { Select } from '@ngxs/store';
import { AppState } from '@store/app.state';
import { UserService } from '@services/user/user.service';

@Injectable({
  providedIn: 'root'
})
export class WindowService {

  @Select(AppState.previousMdtPath) previousMdtPath$!: Observable<string[]>;

  visible: boolean = false; // TODO: this is only needed because of the close event spam
  previousMdtPath: string[] = ['mdt', 'dashboard'];

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private _router: Router,
    private _userService: UserService,
  ) {
    this._event.onEvent<BaseNuiEvent>(NuiEventType.Close).subscribe(() => this.onClose()) // TODO: investigate why this subscription spams after the first close event
    this._event.onEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(() => {
      this._userService.getData();
      this.onMdtOpen();
    });
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
  }

  private onMdtOpen(): void {
    this.show();
    this._router.navigate([...this.previousMdtPath]);
  }

  private onMenuOpen(res: MenuShowEvent): void {
    this.show();
    this._router.navigate(['menu', res.menuId]);
  }
}
