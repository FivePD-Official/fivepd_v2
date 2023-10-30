import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { LooseDataEntity, PedHistory, PedLicense, PedLicenseStatus } from "@models";
import { tap, zip } from "rxjs";
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
    ).subscribe(([tickets, warnings]) => {
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
