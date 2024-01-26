import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from "../../../../components/grid/grid.component";
import { Collation, Column, Page, User } from "@models";
import { BehaviorSubject, filter, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, TranslatePipe, GridComponent],
  templateUrl: './users.component.html',
})
export class UsersComponent {

  private readonly _userService = inject(UserService);
  private readonly _localizationService = inject(LocalizationService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly users$ = this.collation$.pipe(
    switchMap(collation => this._userService.getAllPageable(collation)),
  );

  readonly columns: Column<User>[] = [
    new Column<User>({
      header: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => this._userService.userHasRole(UserRole.Admin),
      accessorKey: 'Name',
    }),
    new Column<User>({
      header: this._localizationService.translate('UI.LABEL.ROLE.SINGULAR'),
      show: () => this._userService.userHasRole(UserRole.Admin),
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
  ];
}
