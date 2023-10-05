import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { PedHistory, PedLicense, PedLicenseStatus } from "@models";
import { tap } from "rxjs";
import { FGender, PedLicenseType } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { addPedToRecentlyViewed } from "@store/app.actions";

@Component({
  selector: 'app-citizen-database-id',
  templateUrl: './citizen-database-id.component.html',
  styleUrls: ['./citizen-database-id.component.sass']
})
export class CitizenDatabaseIdComponent implements OnInit {

  params!: Params;
  pedHistory?: PedHistory;

  constructor(private readonly _activatedRoute: ActivatedRoute,
              private readonly _pedHistoryService: PedHistoryService,
              private readonly _store: Store<GlobalState>) {
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
      this._store.dispatch(addPedToRecentlyViewed({payload: pedHistory}));
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

  protected readonly FGender = FGender;
  protected readonly PedLicenseStatus = PedLicenseStatus;
}
