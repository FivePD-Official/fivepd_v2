import { Component, inject } from '@angular/core';
import { Column, Department, GeneralConfig, IncidentDTO, PlayerDetailsDTO, UnitDTO } from '@models';
import { DepartmentService } from '@services/department/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { UserService } from '@services/user/user.service';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { UnitService } from "@services/unit/unit.service";
import { ConfigService } from "@services/config/config.service";
import { FDutyStatus, FIncidentType } from "@enums";
import { IncidentService } from "@services/incident/incident.service";
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { GridComponent } from '../../../components/grid/grid.component';
import { ContainerComponent } from '../../../components/container/container.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import {
  INCIDENT,
  IncidentDetailsModalComponent
} from "./incidents/incident-details-modal/incident-details-modal.component";
import { BehaviorSubject, filter, map, of, shareReplay, switchMap, tap } from "rxjs";
import { getPriorityBadgeColor, getPriorityLabel } from "../../../helpers/fpriority-helpers";
import { AsyncPipe } from "@angular/common";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.sass'],
    imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, AsyncPipe]
})
export class DashboardComponent {

  readonly _departmentService = inject(DepartmentService);
  readonly _localization = inject(LocalizationService);
  readonly _modalService = inject(ModalService);
  readonly _modalFormService = inject(ModalFormService);
  readonly _modalConfirmService = inject(ModalConfirmService);
  readonly _userService = inject(UserService);
  readonly _store = inject<Store<GlobalState>>(Store);
  readonly _unitService = inject(UnitService);
  readonly _configService = inject(ConfigService);
  readonly _incidentService = inject(IncidentService);

  generalConfig?: GeneralConfig;
  user?: PlayerDetailsDTO;

  readonly generalConfig$ = this._configService.getGeneralConfig().subscribe(res => this.generalConfig = res);
  readonly user$ = this._store.pipe(
    takeUntilDestroyed(),
    tap(state => this.user = state.user.details),
  ).subscribe();

  readonly departmentsCollation$ = new BehaviorSubject({Page: 1, Filters: {}});
  readonly departments$ = this.departmentsCollation$.pipe(
    switchMap(collation => this._departmentService.getAllPageable(collation)),
  );
  readonly departmentsColumnDef: Column<Department>[] = [
    new Column<Department>({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'Name',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.NAME',
      }
    }),
    new Column<Department>({
      header: 'LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.CALLSIGN',
      },
    }),
    new Column<Department>({
      header: 'LABEL.CAPACITY.SINGULAR',
      accessorKey: 'UserCount',
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localization.translate('UI.LABEL.NO_LIMIT')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
  ];

  departmentsRowLinkResolver(item: Department): string {
    return `/mdt/departments/${item.Id}`;
  }

  readonly unitsRefetch$ = new BehaviorSubject<null>(null);
  readonly units$ = this.unitsRefetch$.pipe(
    switchMap(() => this._unitService.getPage()),
  );
  readonly unitsColumnDef: Column<UnitDTO>[] = [
    new Column<UnitDTO>({
      header: 'LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
    }),
    new Column<UnitDTO>({
      header: 'UI.LABEL.OFFICER.PLURAL',
      valueResolver: item => (this.generalConfig?.config?.Unit?.MaxMembers ?? 0) > 0
        ? `${item.Players.length.toString()} / ${this.generalConfig?.config.Unit.MaxMembers}`
        : item.Players.length.toString(),
    }),
    new Column<UnitDTO>({
      header: 'LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'ACTION.JOIN.TITLE',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty &&
            (this.generalConfig?.joinFreelyToUnit() ?? false) &&
            unit.Players.every(x => x.NetworkId !== this.user?.NetworkId) &&
            unit.Players.length > 0 &&
            unit.Players[0].NetworkId !== this.user?.NetworkId &&
            unit.DepartmentId === this.user?.User?.DepartmentId &&
            !unit.AssignedToIncident,
          onClick: unit => {
            this._unitService.join(unit).subscribe(res => {
              if (res.Status) {
                this.unitsRefetch$.next(null);
              }
            });
          }
        },
        {
          type: 'button',
          label: 'ACTION.REQUEST_TO_JOIN.TITLE',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty &&
            (this.generalConfig?.joinByRequestToUnit() ?? false) &&
            unit.Players.every(x => x.NetworkId !== this.user?.NetworkId) &&
            unit.Players.length > 0 &&
            unit.Players[0].NetworkId !== this.user?.NetworkId &&
            unit.DepartmentId === this.user?.User.DepartmentId &&
            !unit.AssignedToIncident,
          onClick: unit => {
            this._unitService.requestToJoin(unit).subscribe();
          }
        },
        {
          type: 'button',
          label: 'ACTION.LEAVE.TITLE',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty && unit.Players.some(x => x.NetworkId === this.user?.NetworkId) && (unit.Players.length > 1),
          onClick: unit => {
            this._unitService.leave(unit).subscribe(res => {
              if (res.Status) {
                this.unitsRefetch$.next(null);
              }
            });
          }
        },
      ]
    }),
  ];

  readonly incidentsRefetch$ = new BehaviorSubject<null>(null);
  readonly incidents$ = this.incidentsRefetch$.pipe(
    switchMap(() => this._incidentService.getPage()),
  );
  readonly incidentsColumnDef: Column<IncidentDTO>[] = [
    new Column({
      header: 'UI.LABEL.TITLE.SINGULAR',
      accessorKey: 'Title',
    }),
    new Column({
      header: 'LABEL.DESCRIPTION.SINGULAR',
      accessorKey: 'Description',
    }),
    new Column({
      header: 'LABEL.ADDRESS.SINGULAR',
      accessorKey: 'Address',
    }),
    new Column({
      header: 'UI.LABEL.UNIT.PLURAL',
      valueResolver: item => item.Units.join(', '),
    }),
    new Column({
      header: 'UI.LABEL.TYPE.SINGULAR',
      valueResolver: item => this._localization.translate(item.Type === FIncidentType.Callout ? 'LABEL.CALLOUT.SINGULAR' : 'UI.LABEL.TRAFFIC_STOP.SINGULAR'),
    }),
    new Column({
      header: 'LABEL.PRIORITY.SINGULAR',
      valueResolver: item => this._localization.translate(getPriorityLabel(item.Priority)),
      badgeResolver: item => getPriorityBadgeColor(item.Priority),
    }),
    new Column({
      header: 'LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: this._localization.translate('LABEL.DETAILS.TITLE'),
          show: () => true,
          onClick: incident => {
            this._modalService.openFromSide(IncidentDetailsModalComponent,
              {
                title: incident.Title,
              },
              new Map().set(INCIDENT, incident))
              .result
              .pipe(
                filter(res => res.success),
                switchMap(res => res.value['close'] ? of(res).pipe(this._modalService.close()) : of(res)),
              )
              .subscribe(() => {
                this.incidentsRefetch$.next(null);
              });
          }
        },
        {
          type: 'button',
          label: this._localization.translate('UI.LABEL.INCIDENT.SELF_ASSIGN.TITLE'),
          show: incident => !!this.user?.UnitCallsign && incident.Units.every(x => x !== this.user?.UnitCallsign),
          onClick: incident => {
            this._incidentService.selfAssign(incident).subscribe(res => {
              if (res.Status) {
                this.incidentsRefetch$.next(null);
              }
            });
          }
        }
      ]
    }),
  ];

  readonly assignedIncidentRefetch$ = new BehaviorSubject<null>(null);
  readonly assignedIncident$ = this.assignedIncidentRefetch$.pipe(
    switchMap(() => this._incidentService.getAssignedIncident()),
    shareReplay(),
  );
  readonly priorityLabel$ = this.assignedIncident$.pipe(
    map(incident => getPriorityLabel(incident.Incident.Priority)),
  );

  protected readonly FIncidentType = FIncidentType;
}
