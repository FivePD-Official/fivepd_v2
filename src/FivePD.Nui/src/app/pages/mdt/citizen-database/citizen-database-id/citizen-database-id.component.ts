import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { AttributeDataType, LooseDataEntity, PedHistory, PedLicense, PedLicenseStatus } from "@models";
import { map, tap, zip } from "rxjs";
import { CoreSetId, FGender, PedLicenseType } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { IconComponent } from '../../../../components/icon/icon.component';
import { DatePipe, NgFor, NgIf } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { AppActions } from "@store/app.actions";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { formatDate } from "@helpers";

@Component({
  selector: 'app-citizen-database-id',
  templateUrl: './citizen-database-id.component.html',
  styleUrls: ['./citizen-database-id.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, NgIf, IconComponent, NgFor, DatePipe, TranslatePipe]
})
export class CitizenDatabaseIdComponent implements OnInit {

  protected readonly FGender = FGender;
  protected readonly PedLicenseStatus = PedLicenseStatus;
  protected readonly ObjectEntries = Object.entries;

  params!: Params;
  pedHistory?: PedHistory;
  tickets: LooseDataEntity[] = [];
  warnings: LooseDataEntity[] = [];

  constructor(private readonly _activatedRoute: ActivatedRoute,
              private readonly _pedHistoryService: PedHistoryService,
              private readonly _store: Store<GlobalState>,
              private readonly _looseDataService: LooseDataService) {
    _activatedRoute.params
      .pipe(
        takeUntilDestroyed(),
        tap(params => this.params = params),
      )
      .subscribe(params => {
        this.getPedHistory(params['id']);
      });
  }

  ngOnInit(): void {
  }

  getPedHistory(id: string): void {
    this._pedHistoryService.get(id).subscribe(pedHistory => {
      this.pedHistory = pedHistory;
      this._store.dispatch(AppActions.addPedToRecentlyViewed({ped: pedHistory}));
    });

    const filter = {
      PedHistoryId: parseInt(id),
    };
    zip(
      this._looseDataService.getEntities(CoreSetId.PedTicket, filter),
      this._looseDataService.getEntities(CoreSetId.PedWarning, filter),
    ).pipe(
      map(entityArrays => {
        return entityArrays.map(entityArray => entityArray.map(entity => {
          // TODO: map core attributes to their names somehow, should be done on the server
          // delete entity.MappedValues['UserId'];
          // delete entity.MappedValues['UserIds'];
          // delete entity.MappedValues['DepartmentId'];
          // delete entity.MappedValues['DepartmentIds'];
          // delete entity.MappedValues['IncidentHistoryId'];
          // delete entity.MappedValues['IncidentHistoryIds'];
          delete entity.MappedValues['PedHistoryId'];
          delete entity.MappedValues['PedHistoryIds'];
          return entity;
        }));
      }),
      map(entityArrays => {
        return entityArrays.map(entityArray => entityArray.map(entity => {
          for (const [key, value] of Object.entries(entity.MappedValues)) {
            const attribute = entity.Set?.Attributes?.find(attribute => attribute.Name === key);
            if (attribute && attribute.DataType === AttributeDataType.Date) {
              entity.MappedValues[key] = formatDate(new Date(value), 'none');
            }
          }
          return entity;
        }));
      }),
    )
      .subscribe(([tickets, warnings]) => {
        console.log(tickets)
        this.tickets = tickets;
        this.warnings = warnings;
      });
  }

  licenseTypeI18nKey(licenseType: PedLicenseType): string {
    switch (licenseType) {
      case PedLicenseType.Driving:
        return 'Driving';
      case PedLicenseType.Fishing:
        return 'Fishing';
      case PedLicenseType.Hunting:
        return 'Hunting';
      case PedLicenseType.Ccw:
        return 'Ccw';
    }
  }

  join(array: any[] = []): string {
    return array.join(', ');
  }

  getLicenseStatusLabel(license: PedLicense): string {
    switch (license.Status) {
      case PedLicenseStatus.Valid:
        return 'LABEL.VALID';
      case PedLicenseStatus.Expired:
        return 'LABEL.EXPIRED';
      case PedLicenseStatus.Suspended:
        return 'LABEL.SUSPENDED';
      case PedLicenseStatus.Revoked:
        return 'LABEL.REVOKED';
    }
  }
}
