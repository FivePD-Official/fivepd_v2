import { Component, OnInit } from '@angular/core';
import { Button, Collation, Column, MappedLooseDataEntity, Page, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { filter, switchMap } from 'rxjs';
import { ATTRIBUTES, ENTITY } from '../../../components/modal/modal-entity-view/entity-token';
import { LocalizationService } from '@services/localization/localization.service';
import { DefaultSetId } from '@enums';
import { Store } from '@ngrx/store';
import { ModalEntityViewComponent } from '../../../components/modal/modal-entity-view/modal-entity-view.component';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { mapAttributesToFields, mapAttributesToGridColumns } from '@helpers';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.sass']
})
export class ReportsComponent implements OnInit {

  user?: User;

  arrestReportButtons: Button[] = [
    {
      type: 'button',
      label: this._localization.translate('UI.ACTION.CREATE'),
      show: () => true,
      disabled: () => !this.user?.Department,
      tooltip: {
        enabled: () => !this.user?.Department,
        text: this._localization.translate('UI.LABEL.ARREST_REPORT.CREATE.TOOLTIP'),
        placement: 'left',
      },
      onClick: () => this.modalFormService.open({
        title: 'UI.ACTION.CREATE',
        buttons: {
          submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
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
    private readonly store: Store<GlobalState>,
    private _looseData: LooseDataService,
    private _localization: LocalizationService,
    private _modal: ModalService,
    private readonly modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
    private readonly _userService: UserService,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.user = state.user.details;
    });
  }

  ngOnInit(): void {
    this.getArrestReports();
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
              label: this._localization.translate('UI.ACTION.VIEW'),
              show: () => true,
              onClick: entity => {
                this._modal.open(ModalEntityViewComponent, {
                  title: 'UI.LABEL.ARREST_REPORT.SINGULAR',
                  columns: 2,
                }, new Map().set(ENTITY, entity).set(ATTRIBUTES, wrapper.attributes));
              }
            },
            {
              type: 'button',
              label: this._localization.translate('UI.ACTION.EDIT'),
              show: () => this._userService.userHasRole(UserRole.Admin),
              onClick: entity => {
                this.modalFormService.open({
                    title: 'UI.ACTION.EDIT',
                    buttons: {
                      submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
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
              label: this._localization.translate('UI.ACTION.DELETE'),
              show: () => this._userService.userHasRole(UserRole.Admin),
              onClick: item => {
                this.modalConfirmService.open({
                  title: 'UI.LABEL.ARREST_REPORT.DELETE.TITLE',
                  description: this._localization.translate('UI.LABEL.ARREST_REPORT.DELETE.DESCRIPTION'),
                  buttons: {
                    submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
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
