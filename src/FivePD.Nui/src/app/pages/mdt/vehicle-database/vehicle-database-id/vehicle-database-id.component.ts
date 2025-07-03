import { Component, inject } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { map, switchMap, tap } from "rxjs";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { ContainerComponent } from "../../../../components/container/container.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { VehicleDocumentStatus, VehicleDocumentType } from "@models";
import { IconComponent } from "../../../../components/icon/icon.component";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { AppActions } from "@store/app.actions";

@Component({
    selector: 'app-vehicle-database-id',
    imports: [PageHeaderComponent, ContainerComponent, TranslatePipe, AsyncPipe, DatePipe, IconComponent, RouterLink],
    templateUrl: './vehicle-database-id.component.html',
    styleUrls: ['./vehicle-database-id.component.sass']
})
export class VehicleDatabaseIdComponent {

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _vehicleService = inject(VehicleService);
  private readonly _store = inject<Store<GlobalState>>(Store<GlobalState>);

  protected readonly VehicleDocumentStatus = VehicleDocumentStatus;

  readonly vehicle$ = this._activatedRoute.params.pipe(
    takeUntilDestroyed(),
    map(params => parseInt(params['id'])),
    switchMap(id => this._vehicleService.get(id)),
    tap(vehicleData => this._store.dispatch(AppActions.addVehicleToRecentlyViewed({vehicle: vehicleData}))),
  );

  licenseTypeI18nKey(licenseType: VehicleDocumentType): string {
    switch (licenseType) {
      case VehicleDocumentType.Insurance:
        return 'Insurance';
      case VehicleDocumentType.Registration:
        return 'Registration';
    }
  }

  getLicenseStatusLabel(status: VehicleDocumentStatus): string {
    switch (status) {
      case VehicleDocumentStatus.Valid:
        return 'LABEL.VALID';
      case VehicleDocumentStatus.Expired:
        return 'LABEL.EXPIRED';
      case VehicleDocumentStatus.Suspended:
        return 'LABEL.SUSPENDED';
      case VehicleDocumentStatus.Revoked:
        return 'LABEL.REVOKED';
    }
  }

}
