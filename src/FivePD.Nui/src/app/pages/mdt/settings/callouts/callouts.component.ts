import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from "../../../../components/grid/grid.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { CalloutDetails, Collation, Column } from "@models";
import { Router } from "@angular/router";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { UserRole, UserService } from "@services/user/user.service";
import { BehaviorSubject, switchMap } from "rxjs";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-callouts',
  standalone: true,
  imports: [CommonModule, GridComponent, TranslatePipe],
  templateUrl: './callouts.component.html',
})
export class CalloutsComponent {

  private readonly _router = inject(Router);
  private readonly _userService = inject(UserService);
  private readonly _calloutDetailsService = inject(CalloutDetailsService);
  private readonly _localizationService = inject(LocalizationService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly calloutDetails$ = this.collation$.pipe(
    switchMap(collation => this._calloutDetailsService.getPage(collation)),
  );

  readonly columns: Column<CalloutDetails>[] = [
    new Column({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'TypeName',
    }),
    new Column({
      header: 'UI.LABEL.ASSEMBLY.NAME.SINGULAR',
      accessorKey: 'AssemblyName',
    }),
    new Column({
      header: 'UI.LABEL.ACTIVE',
      valueResolver: item => this._localizationService.translate(item.Active ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Active ? 'green' : 'red'
    }),
    new Column({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => this._userService.userHasRole(UserRole.Admin),
          onClick: item => this._router.navigate(['mdt', 'settings', 'callouts', item.Id]),
        },
      ],
    })
  ];
}
