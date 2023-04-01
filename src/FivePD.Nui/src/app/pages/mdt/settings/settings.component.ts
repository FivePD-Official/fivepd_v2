import { Component, OnInit } from '@angular/core';
import { AttributeDataType, Button, Collation, Column, Page, SetAttribute, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { filter, Subject, switchMap, takeUntil } from 'rxjs';
import { createAttributesFormFields } from '../../../helpers/forms/set-attribute';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { LocalizationService } from '@services/localization/localization.service';
import { DefaultSetId } from '@enums';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  destroy$ = new Subject<void>();

  user?: User;

  arrestReportAttributes?: Page<SetAttribute>;
  arrestReportAttributesCollation: Collation = {
    Page: 1,
  };
  arrestReportAttributesColumns: Column<SetAttribute>[] = [
    {
      header: this._localization.translate('Nui.Labels.Name'),
      show: () => true,
      accessorKey: 'Name',
    },
    {
      header: this._localization.translate('Nui.Labels.Type'),
      show: () => true,
      valueResolver: attr => {
        switch (attr.DataType) {
          case AttributeDataType.ShortString:
            return this._localization.translate('Nui.Labels.ShortString');
          case AttributeDataType.LongString:
            return this._localization.translate('Nui.Labels.LongString');
          case AttributeDataType.Integer:
            return this._localization.translate('Nui.Labels.WholeNumber');
          case AttributeDataType.Bool:
            return this._localization.translate('Nui.Labels.Checkbox');
          case AttributeDataType.Date:
            return this._localization.translate('Nui.Labels.Date');
          default:
            return '-';
        }
      },
    },
    {
      header: this._localization.translate('Nui.Labels.Required'),
      show: () => true,
      valueResolver: item => this._localization.translate(item.Required ? 'Nui.Labels.Yes' : 'Nui.Labels.No'),
    },
    {
      header: this._localization.translate('Nui.Labels.AddedBy'),
      show: () => true,
      accessorKey: 'AddonId',
    },
    {
      header: this._localization.translate('Nui.Labels.Actions'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Edit'),
          show: item => item.AddonId === 'admin',
          onClick: attribute => {
            this.modalFormService.open({
              title: 'Nui.Labels.EditAttributeTitle',
              buttons: {
                submit: {variant: 'primary', label: 'Nui.Actions.Save'},
              },
              fields: updatedAttribute => createAttributesFormFields(this._localization, updatedAttribute),
              valueResolver: new ValueResolver<SetAttribute>(attribute, this._looseData.getSetAttribute(DefaultSetId.ArrestReport, attribute.Id))
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(res => this._looseData.editSetAttribute(DefaultSetId.ArrestReport, {
                  Id: attribute.Id,
                  Name: res.value['Name'],
                  Label: res.value['Label'],
                  HelpLabel: res.value['HelpLabel'],
                  Placeholder: res.value['Placeholder'],
                  Required: res.value['Required'],
                  ShowInGrid: res.value['ShowInGrid'],
                })),
                this._modal.handleResponse()
              ).subscribe(() => this.getArrestReportAttributes())
          }
        },
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Delete'),
          show: item => item.AddonId === 'admin',
          onClick: item => {
            this.modalConfirmService.open({
              title: 'Nui.Labels.DeleteAttributeTitle',
              description: this._localization.translate('Nui.Labels.DeleteAttributeDescription', {attribute: item.Name}),
              buttons: {
                submit: {variant: 'danger', label: 'Nui.Actions.Delete'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._looseData.deleteSetAttribute(DefaultSetId.ArrestReport, item.Id)),
                this._modal.handleResponse()
              ).subscribe(() => {
              this.arrestReportAttributesCollation.Page = 1;
              this.getArrestReportAttributes();
            })
          }
        }
      ]
    }
  ];
  arrestReportAttributesButtons: Button[] = [
    {
      label: this._localization.translate('Nui.Actions.CreateAttribute'),
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this.modalFormService.open({
          title: 'Nui.Actions.CreateAttribute',
          buttons: {
            submit: {variant: 'primary', label: 'Nui.Actions.Create'},
          },
          fields: () => createAttributesFormFields(this._localization)
        }).result
          .pipe(
            filter(res => res.success),
            switchMap(res => this._looseData.addSetAttribute(DefaultSetId.ArrestReport, {
              Name: res.value['Name'],
              DataType: res.value['DataType'],
              Label: res.value['Label'],
              HelpLabel: res.value['HelpLabel'],
              Placeholder: res.value['Placeholder'],
              Required: res.value['Required'],
              ShowInGrid: res.value['ShowInGrid'],
            })),
            this._modal.handleResponse()
          ).subscribe(() => {
          this.arrestReportAttributesCollation.Page = 1;
          this.getArrestReportAttributes();
        })
      }
    }
  ];

  constructor(
    private _looseData: LooseDataService,
    private _modal: ModalService,
    private readonly modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
    private _localization: LocalizationService,
    private readonly store: Store<GlobalState>,
  ) {
  }

  ngOnInit(): void {
    this.store.pipe(takeUntil(this.destroy$)).subscribe(state => {
      this.user = state.user.details;
      if (state.user.details.Admin) {
        this.getArrestReportAttributes();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.unsubscribe();
  }

  getArrestReportAttributes(): void {
    this._looseData.getSetAttributesPage(DefaultSetId.ArrestReport, this.arrestReportAttributesCollation).subscribe(res => {
      this.arrestReportAttributes = res;
    });
  }

  onArrestReportAttributesCollationChange(collation: Collation): void {
    this.arrestReportAttributesCollation = collation;
    this.getArrestReportAttributes();
  }
}
