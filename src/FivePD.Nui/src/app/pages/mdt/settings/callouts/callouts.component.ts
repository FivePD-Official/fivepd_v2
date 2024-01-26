import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from "../../../../components/grid/grid.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { CalloutDetails, Collation, Column, Page } from "@models";
import { Router } from "@angular/router";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { UserRole, UserService } from "@services/user/user.service";
import { BehaviorSubject, switchMap } from "rxjs";

@Component({
  selector: 'app-callouts',
  standalone: true,
  imports: [CommonModule, GridComponent, TranslatePipe, PageHeaderComponent],
  templateUrl: './callouts.component.html',
})
export class CalloutsComponent {

  private readonly _router = inject(Router);
  private readonly _userService = inject(UserService);
  private readonly _calloutDetailsService = inject(CalloutDetailsService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly calloutDetails$ = this.collation$.pipe(
    switchMap(collation => this._calloutDetailsService.getAll(collation)),
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
      header: 'Active',
      valueResolver: item => item.Active ? 'Yes' : 'No',
      badgeResolver: item => item.Active ? 'green' : 'red'
    }),
    new Column({
      header: 'Actions',
      actions: [
        {
          type: 'button',
          label: 'Edit',
          show: () => this._userService.userHasRole(UserRole.Admin),
          onClick: item => this._router.navigate(['mdt', 'settings', 'callout-details', item.Id]),
        },
      ],
    })
  ];
}
