import { Component, OnDestroy, OnInit } from '@angular/core';
import { Button, Collation, Column, LooseDataEntity, Page, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { SetId } from '@enums';
import { Subject, takeUntil } from 'rxjs';
import { UserState } from '@store/user.state';
import { Store } from '@ngxs/store';
import { mapAttributesToFields, mapAttributesToGridColumns, mapPageEntities } from '@helpers';
import { LocalizationService } from '@services/localization/localization.service';
import { ModalService } from '@services/modal/modal.service';

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
      show: () => !!this.user?.Department,
      onClick: () => this._modal.open({
          title: 'Nui.Actions.Create',
          buttons: {
            cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
            submit: { theme: 'primary', label: 'Nui.Actions.Create' },
          },
          fields: mapAttributesToFields(this._looseData.getSetAttributes(SetId.ArrestReport)),
          formColumns: 2,
        }).onSuccess(modalRes => {
          this._looseData.addEntity(SetId.ArrestReport, {
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
  arrestReports?: Page<LooseDataEntity>;
  arrestReportColumns: Column<LooseDataEntity>[] = [];

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
    this._looseData.getEntities(SetId.ArrestReport, this.arrestReportCollation).subscribe(wrapper => {
      this.arrestReports = mapPageEntities(wrapper.page);
      this.arrestReportColumns = mapAttributesToGridColumns(wrapper.attributes, [
        {
          index: 1,
          item: new Column<LooseDataEntity>({
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
          actions: [
            {
              type: 'button',
              label: this._localization.translate('Nui.Actions.Edit'),
              show: () => true,
              onClick: item => this._looseData.getEntity(SetId.ArrestReport, item.Id).subscribe(newItem => this._modal.open({
                    title: 'Nui.Actions.Edit',
                    buttons: {
                      cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
                      submit: { theme: 'primary', label: 'Nui.Actions.Save' },
                    },
                    fields: mapAttributesToFields(this._looseData.getSetAttributes(SetId.ArrestReport), newItem.entity),
                    formColumns: 2,
                  }).onSuccess(modalRes => {
                    this._looseData.editEntity(SetId.ArrestReport, {
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
              label: this._localization.translate('Nui.Actions.Delete'),
              show: () => true,
              onClick: item => this._modal.open({
                  title: 'Nui.Labels.DeleteArrestReportTitle',
                  description: this._localization.translate('Nui.Labels.DeleteArrestReportDescription'),
                  buttons: {
                    cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
                    submit: { theme: 'danger', label: 'Nui.Actions.Delete' },
                  },
                }).onSuccess(() => {
                  this._looseData.deleteEntity(SetId.ArrestReport, item.Id).subscribe(res => {
                    if (res.Status) {
                      this.getArrestReports();
                    }
                  })
                })
            }
          ],
          show: () => this.user?.Admin ?? false,
        }
      );
    });
  }

  onArrestReportCollationChange(collation: Collation): void {
    this.arrestReportCollation = collation;
    this.getArrestReports();
  }

}
