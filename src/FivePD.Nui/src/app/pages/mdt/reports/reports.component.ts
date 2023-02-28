import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActionResponse, Button, Collation, Column, MappedLooseDataEntity, Page, User, ValueResolver } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DefaultSetId } from '@enums';
import { filter, finalize, iif, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { UserState } from '@store/user.state';
import { Store } from '@ngxs/store';
import { mapAttributesToFields, mapAttributesToGridColumns } from '@helpers';
import { LocalizationService } from '@services/localization/localization.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';
import { ModalEntityViewComponent } from '../../../components/modal/modal-entity-view/modal-entity-view.component';
import { ATTRIBUTES, ENTITY } from '../../../components/modal/modal-entity-view/entity-token';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.sass']
})
export class ReportsComponent implements OnInit, OnDestroy {

  onDestroy$ = new Subject<void>();
  user?: User;

  arrestReportButtons: Button[] = [
    {
      type: 'button',
      label: this._localization.translate('Nui.Actions.Create'),
      show: () => true,
      disabled: () => !this.user?.Department,
      tooltip: {
        enabled: () => !this.user?.Department,
        text: this._localization.translate('Nui.Labels.ArrestReportCreationTooltip'),
        placement: 'left',
      },
      onClick: () => this.modalFormService.open({
        title: 'Nui.Actions.Create',
        buttons: {
          submit: {variant: 'primary', label: 'Nui.Actions.Create'},
        },
        fields: () => mapAttributesToFields(this._looseData.getSetAttributes(DefaultSetId.ArrestReport)),
        columns: 2,
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => this._looseData.addEntity(DefaultSetId.ArrestReport, {
            DepartmentId: this.user!.DepartmentId,
            DateOfSubmit: new Date(),
            ...res.value!,
          })),
          this._modal.handleResponse()
        ).subscribe(() => this.getArrestReports())
    }
  ];
  arrestReportCollation: Collation = {
    Page: 1,
  };
  arrestReports?: Page<MappedLooseDataEntity>;
  arrestReportColumns: Column<MappedLooseDataEntity>[] = [];

  constructor(
    private _store: Store,
    private _looseData: LooseDataService,
    private _localization: LocalizationService,
    private _modal: ModalService,
    private readonly modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
  ) {
  }

  ngOnInit(): void {
    this._store.select(UserState).pipe(
      takeUntil(this.onDestroy$)
    ).subscribe(user => {
      this.user = user.details;
    });
    this.getArrestReports();
  }

  ngOnDestroy(): void {
    this.onDestroy$.next();
    this.onDestroy$.unsubscribe();
  }

  getArrestReports(): void {
    this._looseData.getEntities(DefaultSetId.ArrestReport, this.arrestReportCollation).subscribe(wrapper => {
      this.arrestReports = wrapper.page;
      this.arrestReportColumns = mapAttributesToGridColumns(wrapper.attributes, [
        {
          index: 1,
          item: new Column<MappedLooseDataEntity>({
            header: 'Department',
            valueResolver: item => item['Department'] ? item['Department']['Name'] : undefined,
            linkResolver: item => item['Department'] ? `/mdt/departments/${item['Department']['Id']}` : '#',
            show: () => true,
          })
        }
      ]);
      this.arrestReportColumns.push(
        {
          header: 'Actions',
          show: () => true,
          actions: [
            {
              type: 'button',
              label: this._localization.translate('Nui.Actions.View'),
              show: () => true,
              onClick: entity => {
                this._modal.open(ModalEntityViewComponent, {
                  title: 'Nui.Labels.ArrestReport',
                  columns: 2,
                }, new Map().set(ENTITY, entity).set(ATTRIBUTES, wrapper.attributes));
              }
            },
            {
              type: 'button',
              label: this._localization.translate('Nui.Actions.Edit'),
              show: () => this.user?.Admin ?? false,
              onClick: entity => {
                this.modalFormService.open({
                    title: 'Nui.Actions.Edit',
                    buttons: {
                      submit: {variant: 'primary', label: 'Nui.Actions.Save'},
                    },
                    columns: 2,
                    fields: updatedEntity => mapAttributesToFields(this._looseData.getSetAttributes(entity.SetId), updatedEntity),
                    valueResolver: new ValueResolver(entity, this._looseData.getEntity(entity.SetId, entity.Id), v => v.entity)
                  }
                ).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(res => this._looseData.editEntity(DefaultSetId.ArrestReport, entity.Id, {...entity, ...res.value!})),
                    this._modal.handleResponse(),
                  )
                  .subscribe(() => this.getArrestReports());
              }
            },
            {
              type: 'button',
              label: this._localization.translate('Nui.Actions.Delete'),
              show: () => this.user?.Admin ?? false,
              onClick: item => {
                this.modalConfirmService.open({
                  title: 'Nui.Labels.DeleteArrestReportTitle',
                  description: this._localization.translate('Nui.Labels.DeleteArrestReportDescription'),
                  buttons: {
                    submit: {variant: 'danger', label: 'Nui.Actions.Delete'},
                  },
                }).result
                  .pipe(
                    filter(res => res.success),
                    switchMap(() => this._looseData.deleteEntity(DefaultSetId.ArrestReport, item.Id)),
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

}
