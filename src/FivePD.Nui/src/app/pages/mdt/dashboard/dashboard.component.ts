import { Component, OnInit } from '@angular/core';
import {
  Collation,
  Column,
  Department,
  GeneralConfig,
  IncidentDTO,
  IncidentWrapper,
  Page,
  PlayerDetailsDTO,
  UnitDTO
} from '@models';
import { DepartmentService } from '@services/department/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { UserService } from '@services/user/user.service';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UnitService } from "@services/unit/unit.service";
import { ConfigService } from "@services/config/config.service";
import { FDutyStatus, FIncidentPriority, FIncidentType } from "@enums";
import { IncidentService } from "@services/incident/incident.service";
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { GridComponent } from '../../../components/grid/grid.component';
import { ContainerComponent } from '../../../components/container/container.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { TextFieldComponent } from "../../../components/form-inputs/text-field/text-field.component";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, TextFieldComponent]
})
export class DashboardComponent implements OnInit {

  generalConfig?: GeneralConfig;
  user?: PlayerDetailsDTO;
  assignedIncident?: IncidentWrapper;

  departments?: Page<Department>;
  departmentsCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  departmentsColumnDef: Column<Department>[] = [
    new Column<Department>({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'Name',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.NAME',
      }
    }),
    new Column<Department>({
      header: 'UI.LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.CALLSIGN',
      },
    }),
    new Column<Department>({
      header: 'UI.LABEL.CAPACITY.SINGULAR',
      accessorKey: 'UserCount',
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localization.translate('UI.LABEL.NO_LIMIT')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
  ];

  departmentsRowLinkResolver(item: Department): string {
    return `/mdt/departments/${item.Id}`;
  }

  units?: Page<UnitDTO>;
  unitsColumnDef: Column<UnitDTO>[] = [
    new Column<UnitDTO>({
      header: 'UI.LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
    }),
    new Column<UnitDTO>({
      header: 'UI.LABEL.OFFICER.PLURAL',
      valueResolver: item => (this.generalConfig?.config?.Unit?.MaxMembers ?? 0) > 0
        ? `${item.Players.length.toString()} / ${this.generalConfig?.config.Unit.MaxMembers}`
        : item.Players.length.toString(),
    }),
    new Column<UnitDTO>({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.JOIN',
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
                this.getUnits();
              }
            });
          }
        },
        {
          type: 'button',
          label: 'UI.ACTION.REQUEST_TO_JOIN',
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
          label: 'UI.ACTION.LEAVE',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty && unit.Players.some(x => x.NetworkId === this.user?.NetworkId) && (unit.Players.length > 1),
          onClick: unit => {
            this._unitService.leave(unit).subscribe(res => {
              if (res.Status) {
                this.getUnits();
              }
            });
          }
        },
      ]
    }),
  ];

  incidents?: Page<IncidentDTO>;
  incidentsColumnDef: Column<IncidentDTO>[] = [
    new Column({
      header: 'UI.LABEL.TITLE.SINGULAR',
      accessorKey: 'Title',
    }),
    new Column({
      header: 'UI.LABEL.DESCRIPTION.SINGULAR',
      accessorKey: 'Description',
    }),
    new Column({
      header: 'UI.LABEL.ADDRESS.SINGULAR',
      accessorKey: 'Address',
    }),
    new Column({
      header: 'UI.LABEL.UNIT.PLURAL',
      valueResolver: item => item.Units.join(', '),
    }),
    new Column({
      header: 'UI.LABEL.TYPE.SINGULAR',
      valueResolver: item => item.Type === FIncidentType.Callout ? 'Callout' : 'Traffic stop',
      badgeResolver: item => item.Type === FIncidentType.Callout ? 'amber' : 'blue',
    }),
    new Column({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'Self-assign',
          show: incident => !!this.user?.UnitCallsign && incident.Units.every(x => x !== this.user?.UnitCallsign),
          onClick: incident => {
            this._incidentService.selfAssign(incident).subscribe(res => {
              if (res.Status) {
                incident.Units.push(this.user!.UnitCallsign!);
              }
            });
          }
        }
      ]
    }),
  ];

  constructor(
    private readonly _departmentService: DepartmentService,
    private readonly _localization: LocalizationService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _unitService: UnitService,
    private readonly _configService: ConfigService,
    private readonly _incidentService: IncidentService,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.user = state.user.details;
    });
  }

  ngOnInit(): void {
    this.getGeneralConfig();
    this.load();
  }

  get priorityLabel(): string {
    if (!this.assignedIncident) {
      return '';
    }

    switch (this.assignedIncident.Incident.Priority) {
      case FIncidentPriority.Code1:
        return 'LABEL.PRIORITY.CODE1';
      case FIncidentPriority.Code2:
        return 'LABEL.PRIORITY.CODE2';
      case FIncidentPriority.Code3:
        return 'LABEL.PRIORITY.CODE3';
    }

    return '-';
  }

  getDepartments(): void {
    this._departmentService.getAllPageable(this.departmentsCollation).subscribe(res => {
      this.departments = res;
    });
  }

  private getGeneralConfig(): void {
    this._configService.getGeneralConfig().subscribe(res => {
      this.generalConfig = res;
    });
  }

  private getUnits(): void {
    this._unitService.getPage().subscribe(res => {
      this.units = res;
    });
  }

  private getIncidents(): void {
    this._incidentService.getPage().subscribe(res => {
      this.incidents = res;
    });
  }

  private getAssignedIncident(): void {
    this._incidentService.getAssignedIncident().subscribe(res => {
      this.assignedIncident = res;
    });
  }

  private load(): void {
    this.getDepartments();
    this.getUnits();
    this.getIncidents();
    this.getAssignedIncident();
  }

  onDepartmentsCollationChange(collation: Collation): void {
    this.departmentsCollation = collation;
    this.getDepartments();
  }

  protected readonly FIncidentType = FIncidentType;
}
