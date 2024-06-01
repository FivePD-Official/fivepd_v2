import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from "../../../../components/grid/grid.component";
import { Collation, Column, User } from "@models";
import { BehaviorSubject, filter, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LocalizationService } from "@services/localization/localization.service";
import { AccessControlService } from "@services/access-control/access-control.service";

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, TranslatePipe, GridComponent],
  templateUrl: './users.component.html',
})
export class UsersComponent {

  private readonly _userService = inject(UserService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _accessControlService = inject(AccessControlService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly users$ = this.collation$.pipe(
    switchMap(collation => this._userService.getAllPageable(collation)),
  );

  readonly columns: Column<User>[] = [
    new Column<User>({
      header: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => true,
      accessorKey: 'Name',
    }),
    new Column<User>({
      header: this._localizationService.translate('Access for FivePD'),
      show: () => true,
      valueResolver: user => this._localizationService.translate(user.AccessFivePD ? 'UI.LABEL.ACCESS_CONTROL.ALLOWED' : 'UI.LABEL.ACCESS_CONTROL.NOT_ALLOWED'),
      badgeResolver: user => user.AccessFivePD ? 'green' : 'gray',
    }),
    new Column<User>({
      header: this._localizationService.translate('UI.LABEL.ROLE.SINGULAR'),
      show: () => true,
      actions: [
        {
          type: 'multiSelect',
          label: this._localizationService.translate('UI.LABEL.ROLE.NO'),
          value: () => this._userService.getUserRoleBits(),
          items: () => {
            return [
              {key: UserRole.Admin, label: this._localizationService.translate('UI.LABEL.ROLE.ADMIN')},
              {key: UserRole.Developer, label: this._localizationService.translate('UI.LABEL.ROLE.DEVELOPER')},
              {key: UserRole.Dispatcher, label: this._localizationService.translate('UI.LABEL.ROLE.DISPATCHER')},
            ];
          },
          resetWhenSelectedId: UserRole.None,
          onSelectMulti: (user, selectedIds) => {
            this._userService.changeRole(user, selectedIds)
              .pipe(
                filter(res => res.Status),
              )
              .subscribe(() => {
                this._userService.refreshData()
              });
          }
        }
      ]
    }),
    new Column<User>({
      header: this._localizationService.translate('UI.LABEL.ACTION.PLURAL'),
      actions: [
        {
          type: 'button',
          show: user => user.AccessFivePD,
          label: this._localizationService.translate('UI.LABEL.ACCESS_CONTROL.ACTION.REMOVE_ACCESS'),
          onClick: user => this._accessControlService.disallow(user.Id).subscribe(() => this.collation$.next(this.collation$.value))
        },
        {
          type: 'button',
          show: user => !user.AccessFivePD,
          label: this._localizationService.translate('UI.LABEL.ACCESS_CONTROL.ACTION.GIVE_ACCESS'),
          onClick: user => this._accessControlService.allow(user.Id).subscribe(() => this.collation$.next(this.collation$.value))
        },
      ]
    }),
  ];
}
