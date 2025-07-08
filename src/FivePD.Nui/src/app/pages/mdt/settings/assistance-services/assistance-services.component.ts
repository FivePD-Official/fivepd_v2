import { Component, inject } from '@angular/core';
import { AsyncPipe } from "@angular/common";
import { ContainerComponent } from "../../../../components/container/container.component";
import { GridComponent } from "../../../../components/grid/grid.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { BehaviorSubject, switchMap } from "rxjs";
import { AssistanceServiceTypeConfig, Collation, Column } from "@models";
import { AssistanceServiceService } from "@services/assistance-service/assistance-service.service";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-assistance-services',
  imports: [
    AsyncPipe,
    ContainerComponent,
    GridComponent,
    TranslatePipe
  ],
  templateUrl: './assistance-services.component.html',
})
export class AssistanceServicesComponent {

  private readonly _assistanceServiceService = inject(AssistanceServiceService);
  private readonly _localizationService = inject(LocalizationService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly page$ = this.collation$.pipe(
    switchMap(collation => this._assistanceServiceService.getPage(collation))
  );
  readonly columns: Column<AssistanceServiceTypeConfig>[] = [
    new Column({
      accessorKey: 'TypeName',
      header: 'Type name',
      show: () => true,
      filter: {
        label: 'Search by type name'
      }
    }),
    new Column({
      accessorKey: 'AssemblyName',
      header: 'Assembly name',
      show: () => true,
      filter: {
        label: 'Search by assembly name'
      }
    }),
    new Column({
      header: 'UI.LABEL.ACTIVE',
      valueResolver: item => this._localizationService.translate(item.Active ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Active ? 'green' : 'gray'
    }),
    new Column({
      header: 'UI.LABEL.ENABLED',
      valueResolver: item => this._localizationService.translate(item.Enabled ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Enabled ? 'green' : 'gray'
    }),
  ];

  rowLinkResolver(item: AssistanceServiceTypeConfig): string {
    return `/mdt/settings/assistance-services/type-configs/${item.Id}`;
  }
}
