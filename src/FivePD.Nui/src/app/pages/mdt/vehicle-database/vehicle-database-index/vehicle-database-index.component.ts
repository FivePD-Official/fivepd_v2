import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  Collation,
  Column,
  FormInputBase,
  FormSubmitEvent, FVehicle,
  Page,
  PedHistory,
  SingleSelectField,
  TextInput
} from "@models";
import { formatDate } from "@helpers";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { Router, RouterLink } from "@angular/router";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { zip } from "rxjs";
import { CoreSetId } from "@enums";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { DynamicFormComponent } from "../../../../components/dynamic-form/dynamic-form.component";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { GridComponent } from "../../../../components/grid/grid.component";

@Component({
  selector: 'app-vehicle-database-index',
  standalone: true,
  imports: [CommonModule, TranslatePipe, DynamicFormComponent, PageHeaderComponent, RouterLink, GridComponent],
  templateUrl: './vehicle-database-index.component.html',
  styleUrl: './vehicle-database-index.component.sass'
})
export class VehicleDatabaseIndexComponent implements OnInit {

  vehicles?: Page<FVehicle>;
  collation: Collation = {
    Page: 1,
    Filters: {},
  };
  formFields: FormInputBase<any>[] = [
    new TextInput({
      key: 'LicensePlate',
      label: 'License plate',
      placeholder: '',
    }),
  ];
  columns: Column<FVehicle>[] = [
    new Column({
      header: 'License plate',
      accessorKey: 'LicensePlate',
      show: () => true,
    }),
    new Column({
      header: 'Brand',
      accessorKey: 'Brand',
      show: () => true,
    }),
    new Column({
      header: 'Color',
      accessorKey: 'Color',
      show: () => true,
    }),
    // new Column({
    //   header: 'Lastname',
    //   accessorKey: 'Lastname',
    //   show: () => true,
    // }),
    // new Column({
    //   header: 'Birthdate',
    //   valueResolver: item => formatDate(new Date(item.Birthdate), 'none'),
    //   show: () => true,
    // }),
    // new Column({
    //   header: 'Gender',
    //   valueResolver: item => item.Gender === FGender.Male ? 'Male' : 'Female',
    //   show: () => true,
    // }),
  ];

  recentlyViewedPeds: PedHistory[] = [];

  constructor(private readonly _vehicleService: VehicleService,
              private readonly _router: Router,
              public readonly store: Store<GlobalState>) {
    // store.select('app', 'recentlyViewedPeds').subscribe(peds => this.recentlyViewedPeds = peds);
  }

  ngOnInit(): void {
    this.getAll();
  }

  citizensRowLinkResolver(item: FVehicle): string {
    return `/mdt/vehicle-database/${item.Id}`;
  }

  onSearchSubmit(event: FormSubmitEvent): void {
    this.collation.Filters = event.value as Record<string, string>;
    this.getAll();
  }

  getAll(): void {
    this._vehicleService.getAll(this.collation).subscribe(res => {
      this.vehicles = res;
    });
  }

  onCollationChange(collation: Collation): void {
    this.collation = collation;
    this.getAll();
  }
}
