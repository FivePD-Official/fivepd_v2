import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { PedLicense, PedLicenseStatus } from "@models";
import { BehaviorSubject, combineLatest, filter, map, of, switchMap, tap } from "rxjs";
import { CoreSetId, FGender, PedLicenseType, SetId } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { IconComponent } from '../../../../components/icon/icon.component';
import { AsyncPipe, DatePipe } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { AppActions } from "@store/app.actions";
import { mapAttributesToFields } from "@helpers";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { EntityViewComponent } from "./_components/entity-view.component";
import { parseId } from "../../../../helpers/parse-id";
import { VariableDataService } from "@services/variable-data/variable-data.service";

@Component({
    selector: 'app-citizen-database-id',
    templateUrl: './citizen-database-id.component.html',
    styleUrls: ['./citizen-database-id.component.sass'],
    imports: [PageHeaderComponent, ContainerComponent, IconComponent, DatePipe, TranslatePipe, EntityViewComponent, AsyncPipe, RouterLink]
})
export class CitizenDatabaseIdComponent {

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _pedHistoryService = inject(PedHistoryService);
  private readonly _store = inject<Store<GlobalState>>(Store<GlobalState>);
  private readonly _variableDataService = inject(VariableDataService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);

  protected readonly CoreSetId = CoreSetId;
  protected readonly FGender = FGender;
  protected readonly PedLicenseStatus = PedLicenseStatus;

  readonly id$ = new BehaviorSubject<number>(0);
  readonly ped$ = this._activatedRoute.params
    .pipe(
      takeUntilDestroyed(),
      map(params => parseId(params['id'])),
      tap(id => this.id$.next(id)),
      switchMap(id => this._pedHistoryService.get(id)),
      tap(pedHistory => this._store.dispatch(AppActions.addPedToRecentlyViewed({ped: pedHistory}))),
    );

  readonly ticketsRefresh$ = new BehaviorSubject<void>(undefined);
  readonly tickets$ = combineLatest([
    this.id$,
    this.ticketsRefresh$,
  ]).pipe(
    switchMap(([id]) => this._variableDataService.getEntities(CoreSetId.PedTicket, {PedHistoryId: id})),
    map(entities => entities.map(x => this._variableDataService.mapEntityValues({
      entity: x,
      attributes: this._variableDataService.getSetAttributes(CoreSetId.PedTicket),
    }))),
  );

  readonly warningsRefresh$ = new BehaviorSubject<void>(undefined);
  readonly warnings$ = combineLatest([
    this.id$,
    this.warningsRefresh$,
  ]).pipe(
    switchMap(([id]) => this._variableDataService.getEntities(CoreSetId.PedWarning, {PedHistoryId: id})),
    map(entities => entities.map(x => this._variableDataService.mapEntityValues({
      entity: x,
      attributes: this._variableDataService.getSetAttributes(CoreSetId.PedWarning),
    }))),
  );

  readonly arrestReports$ = this.id$.pipe(
    switchMap(id => this._variableDataService.getEntities(CoreSetId.ArrestReport, {PedHistoryIds: [id]})),
    map(entities => entities.map(x => this._variableDataService.mapEntityValues({
      entity: x,
      attributes: this._variableDataService.getSetAttributes(CoreSetId.ArrestReport),
    }))),
  );

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

  tryCreateVariableDataEntity(setId: SetId): void {
    of(this._variableDataService.getSetAttributes(setId))
      .pipe(
        switchMap(attributes => {
          if (attributes) {
            return of(attributes);
          }

          return this._variableDataService.updateSetAttributes(setId);
        }),
        switchMap(attributes => {
          return this._modalFormService.open({
            title: 'UI.ACTION.CREATE',
            buttons: {
              submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
            },
            fields: () => mapAttributesToFields(attributes),
          }).result.pipe(res => {
            return res;
          })
        }),
        filter(res => res.success),
        switchMap(res => {
          return this._variableDataService.addEntity(setId, res.value);
        }),
        this._modalService.handleResponse()
      )
      .subscribe(() => {
        if (setId === CoreSetId.PedTicket) {
          this.ticketsRefresh$.next();
        }
        if (setId === CoreSetId.PedWarning) {
          this.warningsRefresh$.next();
        }
      });
  }
}
