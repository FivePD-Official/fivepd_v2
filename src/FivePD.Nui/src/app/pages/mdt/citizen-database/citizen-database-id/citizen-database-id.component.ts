import { Component, inject } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { PedHistory, PedLicense, PedLicenseStatus } from "@models";
import { filter, of, switchMap } from "rxjs";
import { CoreSetId, FGender, PedLicenseType, SetId } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { IconComponent } from '../../../../components/icon/icon.component';
import { DatePipe, NgFor, NgIf } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { AppActions } from "@store/app.actions";
import { EntityValue, LooseDataService } from "@services/loose-data/loose-data.service";
import { mapAttributesToFields } from "@helpers";
import { ButtonComponent } from "../../../../components/button/button.component";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { EntityViewComponent } from "./_components/entity-view.component";

@Component({
  selector: 'app-citizen-database-id',
  templateUrl: './citizen-database-id.component.html',
  styleUrls: ['./citizen-database-id.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, NgIf, IconComponent, NgFor, DatePipe, TranslatePipe, ButtonComponent, EntityViewComponent]
})
export class CitizenDatabaseIdComponent {

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _pedHistoryService = inject(PedHistoryService);
  private readonly _store = inject(Store<GlobalState>);
  private readonly _looseDataService = inject(LooseDataService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);

  protected readonly CoreSetId = CoreSetId;
  protected readonly FGender = FGender;
  protected readonly PedLicenseStatus = PedLicenseStatus;

  pedHistory?: PedHistory;
  tickets: EntityValue[][] = [];
  warnings: EntityValue[][] = [];
  arrestReports: EntityValue[][] = [];

  private readonly filter = {
    PedHistoryId: 0,
  };
  private readonly filterPlural = {
    PedHistoryIds: [] as number[],
  };

  constructor() {
    this._activatedRoute.params
      .pipe(
        takeUntilDestroyed(),
      )
      .subscribe(params => {
        const id = parseInt(params['id']);
        this.filter.PedHistoryId = id;
        this.filterPlural.PedHistoryIds = [id];
        this.getPedHistory(id);
      });
  }

  getPedHistory(id: number): void {
    this._pedHistoryService.get(id).subscribe(pedHistory => {
      this.pedHistory = pedHistory;
      this._store.dispatch(AppActions.addPedToRecentlyViewed({ped: pedHistory}));
    });
    this.getTickets();
    this.getWarnings();
    this.getArrestReports();
  }

  getTickets(): void {
    this._looseDataService.getEntities(CoreSetId.PedTicket, this.filter).subscribe(entities => {
      this.tickets = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.PedTicket),
      }));
    });
  }

  getWarnings(): void {
    this._looseDataService.getEntities(CoreSetId.PedWarning, this.filter).subscribe(entities => {
      this.warnings = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.PedWarning),
      }));
    });
  }

  getArrestReports(): void {
    this._looseDataService.getEntities(CoreSetId.ArrestReport, this.filterPlural).subscribe(entities => {
      this.arrestReports = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.ArrestReport),
      }));
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

  tryCreateLooseDataEntity(setId: SetId): void {
    of(this._looseDataService.getSetAttributes(setId))
      .pipe(
        switchMap(attributes => {
          if (attributes) {
            return of(attributes);
          }

          return this._looseDataService.updateSetAttributes(setId);
        }),
        switchMap(attributes => {
          return this._modalFormService.open({
            title: 'UI.ACTION.CREATE',
            buttons: {
              submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
            },
            fields: () => mapAttributesToFields(attributes),
          }).result;
        }),
        filter(res => res.success),
        switchMap(res => this._looseDataService.addEntity(setId, res.value)),
        this._modalService.handleResponse()
      )
      .subscribe(() => {
        if (setId === CoreSetId.PedTicket) {
          this.getTickets();
        }
        if (setId === CoreSetId.PedWarning) {
          this.getWarnings();
        }
      });
  }
}
