import { Component, OnInit } from '@angular/core';
import {
  Button,
  Collation,
  Column,
  DropdownItem,
  MappedVariableDataEntity,
  Page,
  SimplePedHistory,
  User
} from '@models';
import { filter, map, switchMap } from 'rxjs';
import { ATTRIBUTES, ENTITY } from '../../../components/modal/modal-entity-view/entity-token';
import { LocalizationService } from '@services/localization/localization.service';
import { CoreSetId, FGender } from '@enums';
import { Store } from '@ngrx/store';
import { ModalEntityViewComponent } from '../../../components/modal/modal-entity-view/modal-entity-view.component';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { formatDate, mapCoreAttributesToFields, mapAttributesToGridColumns } from '@helpers';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { GridComponent } from '../../../components/grid/grid.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { VariableDataService } from "@services/variable-data/variable-data.service";
import { DataResolver } from "@services/modals/modal-form/modal-form-data-resolver";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.sass'],
  imports: [PageHeaderComponent, GridComponent, TranslatePipe]
})
export class ReportsComponent implements OnInit {

  user?: User;

  arrestReportButtons: Button[] = [
    {
      type: 'button',
      label: this._localization.translate('ACTION.CREATE.TITLE'),
      show: () => true,
      disabled: () => !this.user?.Department,
      tooltip: {
        enabled: () => !this.user?.Department,
        text: this._localization.translate('LABEL.ARREST_REPORT.CREATE.TOOLTIP'),
        placement: 'left',
      },
      onClick: () => this.modalFormService.open({
        title: 'ACTION.CREATE.TITLE',
        buttons: {
          submit: {variant: 'primary', label: 'ACTION.CREATE.TITLE'},
        },
        fields: (_, _2, coreAttributeDropdownItems) => mapCoreAttributesToFields({
          attributes: this._variableDataService.getSetAttributes(CoreSetId.ArrestReport),
          coreAttributeDropdownItems
        }),
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => this._variableDataService.addEntity(CoreSetId.ArrestReport, {
            ...res.value!,
          })),
          this._modal.handleResponse()
        ).subscribe(() => this.getArrestReports())
    }
  ];
  arrestReportCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  arrestReports?: Page<MappedVariableDataEntity>;
  arrestReportColumns: Column<MappedVariableDataEntity>[] = [];

  incidentReportButtons: Button[] = [
    {
      type: 'button',
      label: this._localization.translate('ACTION.CREATE.TITLE'),
      show: () => true,
      disabled: () => !this.user?.Department,
      tooltip: {
        enabled: () => !this.user?.Department,
        text: this._localization.translate('LABEL.ARREST_REPORT.CREATE.TOOLTIP'),
        placement: 'left',
      },
      onClick: () => this.modalFormService.open({
        title: 'ACTION.CREATE.TITLE',
        buttons: {
          submit: {variant: 'primary', label: 'ACTION.CREATE.TITLE'},
        },
        fields: (_, _2, coreAttributeDropdownItems) => mapCoreAttributesToFields({
          attributes: this._variableDataService.getSetAttributes(CoreSetId.IncidentReport),
          coreAttributeDropdownItems
        })
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => this._variableDataService.addEntity(CoreSetId.IncidentReport, {
            ...res.value!,
          })),
          this._modal.handleResponse()
        ).subscribe(() => this.getIncidentReports())
    }
  ];
  incidentReportCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  incidentReports?: Page<MappedVariableDataEntity>;
  incidentReportColumns: Column<MappedVariableDataEntity>[] = [];

  constructor(
    private readonly store: Store<GlobalState>,
    private readonly _variableDataService: VariableDataService,
    private readonly _localization: LocalizationService,
    private readonly _modal: ModalService,
    private readonly modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
    private readonly _userService: UserService,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }


  ngOnInit(): void {
    this.getArrestReports();
    this.getIncidentReports();
  }

  getArrestReports(): void {
    this._variableDataService.getEntitiesPageable(CoreSetId.ArrestReport, this.arrestReportCollation).subscribe(wrapper => {
      this.arrestReports = wrapper.page;
      this.arrestReportColumns = mapAttributesToGridColumns(wrapper.attributes, [
        {
          index: 0,
          item: new Column<MappedVariableDataEntity>({
            header: 'UI.LABEL.ID.SHORT.SINGULAR',
            valueResolver: item => item.Id.toString(),
          }),
        },
      ]);
      this.arrestReportColumns.push(
        {
          header: 'Actions',
          show: () => true,
          actions: [
            {
              type: 'button',
              label: this._localization.translate('ACTION.VIEW.TITLE'),
              show: () => true,
              onClick: entity => {
                this._modal.open(ModalEntityViewComponent, {
                  title: 'LABEL.ARREST_REPORT.SINGULAR',
                  columns: 2,
                }, new Map().set(ENTITY, entity).set(ATTRIBUTES, wrapper.attributes))
                  .result
                  .pipe(this._modal.close())
                  .subscribe();
              }
            },
            {
              type: 'button',
              label: this._localization.translate('ACTION.EDIT.TITLE'),
              show: () => this._userService.userIsDepartmentManagerOrAdmin(),
              onClick: entity => {
                this.modalFormService.open({
                    title: 'ACTION.EDIT.TITLE',
                    buttons: {
                      submit: {variant: 'primary', label: 'ACTION.SAVE.TITLE'},
                    },
                    columns: 2,
                    fields: (updatedEntity, _, coreAttributeDropdownItems) => mapCoreAttributesToFields({
                      attributes: this._variableDataService.getSetAttributes(entity.SetId),
                      entity: updatedEntity,
                      coreAttributeDropdownItems
                    }),
                    valueResolver: new ValueResolver(entity, this._variableDataService.getEntity(entity.SetId, entity.Id), v => v.entity)
                  }
                ).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(res => this._variableDataService.editEntity(CoreSetId.ArrestReport, entity.Id, {...entity, ...res.value!})),
                    this._modal.handleResponse(),
                  )
                  .subscribe(() => this.getArrestReports());
              }
            },
            {
              type: 'button',
              label: this._localization.translate('ACTION.DELETE.TITLE'),
              show: () => this._userService.userIsDepartmentManagerOrAdmin(),
              onClick: item => {
                this.modalConfirmService.open({
                  title: 'LABEL.ARREST_REPORT.DELETE.TITLE',
                  description: this._localization.translate('LABEL.ARREST_REPORT.DELETE.DESCRIPTION'),
                  buttons: {
                    submit: {variant: 'danger', label: 'ACTION.DELETE.TITLE'},
                  },
                }).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(() => this._variableDataService.deleteEntity(CoreSetId.ArrestReport, item.Id)),
                    this._modal.handleResponse()
                  ).subscribe(() => this.getArrestReports())
              }
            }
          ],
        }
      );
    });
  }

  onArrestReportCollationChange(collation: Collation): void {
    this.arrestReportCollation = collation;
    this.getArrestReports();
  }

  getIncidentReports(): void {
    this._variableDataService.getEntitiesPageable(CoreSetId.IncidentReport, this.incidentReportCollation).subscribe(wrapper => {
      this.incidentReports = wrapper.page;
      this.incidentReportColumns = mapAttributesToGridColumns(wrapper.attributes, [
        {
          index: 0,
          item: new Column<MappedVariableDataEntity>({
            header: 'UI.LABEL.ID.SHORT.SINGULAR',
            valueResolver: item => item.Id.toString(),
          }),
        },
        // {
        //   index: 2,
        //   item: new Column<MappedVariableDataEntity>({
        //     header: 'Submitted by',
        //     valueResolver: item => item['User'] ? item['User']['Name'] : '',
        //   }),
        // },
      ]);
      this.incidentReportColumns.push(
        {
          header: 'Actions',
          show: () => true,
          actions: [
            {
              type: 'button',
              label: this._localization.translate('ACTION.VIEW.TITLE'),
              show: () => true,
              onClick: entity => {
                this._modal.open(ModalEntityViewComponent, {
                  title: 'UI.LABEL.INCIDENT_REPORT.SINGULAR',
                }, new Map().set(ENTITY, entity).set(ATTRIBUTES, wrapper.attributes))
                  .result
                  .pipe(this._modal.close())
                  .subscribe();
              }
            },
            {
              type: 'button',
              label: this._localization.translate('ACTION.EDIT.TITLE'),
              show: () => this._userService.userIsDepartmentManagerOrAdmin(),
              onClick: entity => {
                this.modalFormService.open({
                    title: 'ACTION.EDIT.TITLE',
                    buttons: {
                      submit: {variant: 'primary', label: 'ACTION.SAVE.TITLE'},
                    },
                    fields: (updatedEntity, _, coreAttributeDropdownItems) => mapCoreAttributesToFields({
                      attributes: this._variableDataService.getSetAttributes(entity.SetId),
                      entity: updatedEntity,
                      coreAttributeDropdownItems
                    }),
                    valueResolver: new ValueResolver(entity, this._variableDataService.getEntity(entity.SetId, entity.Id), v => v.entity)
                  }
                ).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(res => this._variableDataService.editEntity(CoreSetId.IncidentReport, entity.Id, {...entity, ...res.value!})),
                    this._modal.handleResponse(),
                  )
                  .subscribe(() => this.getIncidentReports());
              }
            },
            {
              type: 'button',
              label: this._localization.translate('ACTION.DELETE.TITLE'),
              show: () => this._userService.userIsDepartmentManagerOrAdmin(),
              onClick: item => {
                this.modalConfirmService.open({
                  title: 'UI.LABEL.INCIDENT_REPORT.DELETE.TITLE',
                  description: this._localization.translate('UI.LABEL.INCIDENT_REPORT.DELETE.DESCRIPTION'),
                  buttons: {
                    submit: {variant: 'danger', label: 'ACTION.DELETE.TITLE'},
                  },
                }).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(() => this._variableDataService.deleteEntity(CoreSetId.IncidentReport, item.Id)),
                    this._modal.handleResponse()
                  ).subscribe(() => this.getIncidentReports())
              }
            }
          ],
        }
      );
    });
  }

  onIncidentReportCollationChange(collation: Collation): void {
    this.incidentReportCollation = collation;
    this.getIncidentReports();
  }

}
