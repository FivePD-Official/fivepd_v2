import { Component, inject } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { map, switchMap, tap } from "rxjs";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { ContainerComponent } from "../../../../components/container/container.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { PedLicenseStatus, VehicleDocumentStatus, VehicleDocumentType } from "@models";
import { IconComponent } from "../../../../components/icon/icon.component";

@Component({
  selector: 'app-vehicle-database-id',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, TranslatePipe, AsyncPipe, DatePipe, IconComponent, RouterLink],
  templateUrl: './vehicle-database-id.component.html',
  styleUrls: ['./vehicle-database-id.component.sass']
})
export class VehicleDatabaseIdComponent {

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _vehicleService = inject(VehicleService);

  protected readonly PedLicenseStatus = PedLicenseStatus;
  protected readonly VehicleDocumentStatus = VehicleDocumentStatus;

  readonly vehicle$ = this._activatedRoute.params.pipe(
    takeUntilDestroyed(),
    map(params => parseInt(params['id'])),
    switchMap(id => this._vehicleService.get(id)),
    tap(x => console.log(x)),
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
