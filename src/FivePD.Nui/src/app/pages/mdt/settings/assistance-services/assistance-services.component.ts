import { Component, inject } from '@angular/core';
import { AsyncPipe } from "@angular/common";
import { ContainerComponent } from "../../../../components/container/container.component";
import { GridComponent } from "../../../../components/grid/grid.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { BehaviorSubject, switchMap } from "rxjs";
import { AssistanceServiceTypeConfig, Collation, Column } from "@models";
import { AssistanceServiceService } from "@services/assistance-service/assistance-service.service";

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

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly page$ = this.collation$.pipe(
    switchMap(collation => this._assistanceServiceService.getPage(collation))
  );
  readonly columns: Column<AssistanceServiceTypeConfig>[] = [
    {
      accessorKey: 'TypeName',
      header: 'Type name',
      show: () => true,
      filter: {
        label: 'Search by type name'
      }
    },
    {
      accessorKey: 'AssemblyName',
      header: 'Assembly name',
      show: () => true,
      filter: {
        label: 'Search by assembly name'
      }
    },
  ];

  rowLinkResolver(item: AssistanceServiceTypeConfig): string {
    return `/mdt/settings/assistance-services/type-configs/${item.Id}`;
  }
}
