import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from "../../../../components/grid/grid.component";
import { AssistanceServiceTypeConfig, CalloutDetails, Collation, Column } from "@models";
import { Router } from "@angular/router";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { UserRole, UserService } from "@services/user/user.service";
import { BehaviorSubject, switchMap } from "rxjs";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
    selector: 'app-callouts',
    imports: [CommonModule, GridComponent],
    templateUrl: './callouts.component.html'
})
export class CalloutsComponent {

  protected readonly _userService = inject(UserService);
  private readonly _router = inject(Router);
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
      header: 'UI.LABEL.ASSEMBLY_NAME.SINGULAR',
      accessorKey: 'AssemblyName',
    }),
    new Column({
      header: 'UI.LABEL.ACTIVE',
      valueResolver: item => this._localizationService.translate(item.Active ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Active ? 'green' : 'red'
    }),
  ];

  rowLinkResolver(item: CalloutDetails): string {
    return `/mdt/settings/callouts/${item.Id}`;
  }

  protected readonly UserRole = UserRole;
}
