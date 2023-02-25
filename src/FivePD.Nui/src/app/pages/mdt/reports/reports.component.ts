import { Component, OnDestroy, OnInit } from '@angular/core';
import { Button, Collation, Column, MappedLooseDataEntity, Page, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DefaultSetId, SetId } from '@enums';
import { Subject, takeUntil } from 'rxjs';
import { UserState } from '@store/user.state';
import { Store } from '@ngxs/store';
import { mapAttributesToFields, mapAttributesToGridColumns, mapPageEntities } from '@helpers';
import { LocalizationService } from '@services/localization/localization.service';
import { ModalService } from '@services/modal/modal.service';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';
import { SidebarComponent } from '../../../components/sidebar/sidebar.component';
import { ModalEntityViewComponent } from '../../../components/modal/modal-entity-view/modal-entity-view.component';
import { ENTITY } from '../../../components/modal/modal-entity-view/entity-token';

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
      label: this._localization.translate('UI.ACTION.CREATE'),
      show: () => true,
      disabled: () => !this.user?.Department,
      tooltip: {
        enabled: () => !this.user?.Department,
        text: this._localization.translate('UI.LABEL.ARREST_REPORT.CREATE.TOOLTIP'),
        placement: 'left',
      },
      onClick: () => this._modal.open(ModalFormComponent, {
          title: 'UI.ACTION.CREATE',
          buttons: {
            cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
            submit: { variant: 'primary', label: 'UI.ACTION.CREATE' },
          },
          fields: mapAttributesToFields(this._looseData.getSetAttributes(DefaultSetId.ArrestReport)),
          columns: 2,
        }).onSuccess(modalRes => {
          this._looseData.addEntity(DefaultSetId.ArrestReport, {
            DepartmentId: this.user!.DepartmentId,
            DateOfSubmit: new Date(),
            ...modalRes.value!,
          }).subscribe(res => {
            if (res.Status) {
              this.getArrestReports();
            }
          });
        })
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
  ) { }

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
      this.arrestReports = mapPageEntities(wrapper.page);
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
                }, new Map<any, any>().set(ENTITY, entity));
              }
            },
            {
              type: 'button',
              label: this._localization.translate('UI.ACTION.EDIT'),
              show: () => this.user?.Admin ?? false,
              onClick: item => this._looseData.getEntity(DefaultSetId.ArrestReport, item.Id).subscribe(newItem => this._modal.open(ModalFormComponent, {
                    title: 'UI.ACTION.EDIT',
                    buttons: {
                      cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
                      submit: { variant: 'primary', label: 'UI.ACTION.SAVE' },
                    },
                    fields: mapAttributesToFields(this._looseData.getSetAttributes(DefaultSetId.ArrestReport), newItem.entity),
                    columns: 2,
                  }).onSuccess(modalRes => {
                    this._looseData.editEntity(DefaultSetId.ArrestReport, newItem.entity.Id, {
                      ...newItem.entity,
                      ...modalRes.value!,
                    }).subscribe(res => {
                      if (res.Status) {
                        this.getArrestReports();
                      }
                    });
                  })
                )
            },
            {
              type: 'button',
              label: this._localization.translate('UI.ACTION.DELETE'),
              show: () => this.user?.Admin ?? false,
              onClick: item => this._modal.open(ModalFormComponent, {
                  title: 'UI.LABEL.ARREST_REPORT.DELETE.TITLE',
                  description: this._localization.translate('UI.LABEL.ARREST_REPORT.DELETE.DESCRIPTION'),
                  buttons: {
                    cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
                    submit: { variant: 'danger', label: 'UI.ACTION.DELETE' },
                  },
                }).onSuccess(() => {
                  this._looseData.deleteEntity(DefaultSetId.ArrestReport, item.Id).subscribe(res => {
                    if (res.Status) {
                      this.getArrestReports();
                    }
                  })
                })
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
