import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { map, switchMap } from "rxjs";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { ContainerComponent } from "../../../../components/container/container.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";

@Component({
  selector: 'app-vehicle-database-id',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, TranslatePipe, AsyncPipe],
  templateUrl: './vehicle-database-id.component.html',
})
export class VehicleDatabaseIdComponent {

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _vehicleService = inject(VehicleService);

  readonly vehicle$ = this._activatedRoute.params.pipe(
    takeUntilDestroyed(),
    map(params => parseInt(params['id'])),
    switchMap(id => this._vehicleService.get(id)),
  );
}
