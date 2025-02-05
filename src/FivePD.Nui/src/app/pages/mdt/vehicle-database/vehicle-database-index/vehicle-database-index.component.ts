import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { Collation, Column, FormInputBase, TextField, VehicleData } from "@models";
import { BehaviorSubject, switchMap, tap } from "rxjs";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { DynamicFormComponent } from "../../../../components/dynamic-form/dynamic-form.component";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { GridComponent } from "../../../../components/grid/grid.component";

@Component({
  selector: 'app-vehicle-database-index',
  standalone: true,
  imports: [AsyncPipe, TranslatePipe, DynamicFormComponent, PageHeaderComponent, GridComponent],
  templateUrl: './vehicle-database-index.component.html',
  styleUrl: './vehicle-database-index.component.sass'
})
export class VehicleDatabaseIndexComponent {

  private readonly _vehicleService = inject(VehicleService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly vehicles$ = this.collation$.pipe(
    switchMap(collation => this._vehicleService.getPage(collation)),
  );

  readonly formFields: FormInputBase<any>[] = [
    new TextField({
      key: 'LicensePlate',
      label: 'LABEL.LICENSE_PLATE.SINGULAR',
      placeholder: '',
    }),
    new TextField({
      key: 'Brand',
      label: 'LABEL.BRAND.SINGULAR',
      placeholder: '',
    }),
    new TextField({
      key: 'Color',
      label: 'LABEL.COLOR.SINGULAR',
      placeholder: '',
    }),
  ];
  readonly columns: Column<VehicleData>[] = [
    new Column({
      header: 'LABEL.LICENSE_PLATE.SINGULAR',
      accessorKey: 'LicensePlate',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.BRAND.SINGULAR',
      accessorKey: 'Brand',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.COLOR.SINGULAR',
      accessorKey: 'Color',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.OWNER_NAME',
      show: () => true,
      valueResolver: item => item.PedData ? `${item.PedData.Firstname} ${item.PedData.Lastname}` : '-',
    })
  ];

  rowLinkResolver(item: VehicleData): string {
    return `/mdt/vehicle-database/${item.Id}`;
  }
}
