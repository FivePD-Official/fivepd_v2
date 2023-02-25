import { Component, OnInit } from '@angular/core';
import {
  AttributeDataType,
  Button,
  Checkbox,
  Collation,
  Column,
  Dropdown,
  Page,
  SetAttribute,
  TextInput,
  User
} from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { DefaultSetId, SetId } from '@enums';
import { ModalService } from '@services/modal/modal.service';
import { UserState, UserStateModel } from '@store/user.state';
import { Subject, takeUntil } from 'rxjs';
import { Store } from '@ngxs/store';
import { LocalizationService } from '@services/localization/localization.service';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  onDestroy$ = new Subject<void>();

  user?: User;

  arrestReportAttributes?: Page<SetAttribute>;
  arrestReportAttributesCollation: Collation = {
    Page: 1,
  };
  arrestReportAttributesColumns: Column<SetAttribute>[] = [
    {
      header: this._localization.translate('UI.LABEL.NAME'),
      show: () => true,
      accessorKey: 'Name',
    },
    {
      header: this._localization.translate('UI.LABEL.TYPE'),
      show: () => true,
      valueResolver: attr => {
        switch (attr.DataType) {
          case AttributeDataType.ShortString: return this._localization.translate('UI.LABEL.SHORT_TEXT');
          case AttributeDataType.LongString: return this._localization.translate('UI.LABEL.LONG_TEXT');
          case AttributeDataType.Integer: return this._localization.translate('UI.LABEL.WHOLE_NUMBER');
          case AttributeDataType.Bool: return this._localization.translate('UI.LABEL.CHECKBOX');
          case AttributeDataType.Date: return this._localization.translate('UI.LABEL.DATE');
          default: return '-';
        }
      },
    },
    {
      header: this._localization.translate('UI.LABEL.REQUIRED'),
      show: () => true,
      valueResolver: item => this._localization.translate(item.Required ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
    },
    {
      header: this._localization.translate('UI.LABEL.ADDED_BY'),
      show: () => true,
      accessorKey: 'AddonId',
    },
    {
      header: this._localization.translate('UI.LABEL.ACTIONS'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          show: item => item.AddonId === 'admin',
          onClick: item => this._modal.open(ModalFormComponent, {
            title: 'UI.LABEL.ATTRIBUTE.EDIT',
            buttons: {
              cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
              submit: { variant: 'primary', label: 'UI.ACTION.SAVE' },
            },
            fields: [
              new TextInput({
                key: 'Name',
                label: 'Name',
                required: true,
                value: item.Name
              }),
              new Dropdown({
                key: 'DataType',
                label: 'Type',
                required: true,
                readonly: true,
                dropdownOptions: {
                  size: 'normal',
                  label: this._localization.translate('UI.LABEL.TYPE'),
                  selectedItemResolver: itemKey => item.DataType === itemKey,
                  items: [
                    { key: AttributeDataType.ShortString, label: this._localization.translate('UI.LABEL.SHORT_TEXT') },
                    { key: AttributeDataType.LongString, label: this._localization.translate('UI.LABEL.LONG_TEXT') },
                    { key: AttributeDataType.Integer, label: this._localization.translate('UI.LABEL.WHOLE_NUMBER') },
                    { key: AttributeDataType.Bool, label: this._localization.translate('UI.LABEL.CHECKBOX') },
                    { key: AttributeDataType.Date, label: this._localization.translate('UI.LABEL.DATE') },
                  ]
                }
              }),
              new TextInput({
                key: 'Label',
                label: this._localization.translate('UI.LABEL.LABEL'),
                required: true,
                value: item.Label
              }),
              new TextInput({
                key: 'HelpLabel',
                label: this._localization.translate('UI.LABEL.HELP_LABEL'),
                required: false,
                value: item.HelpLabel
              }),
              new TextInput({
                key: 'Placeholder',
                label: this._localization.translate('UI.LABEL.PLACEHOLDER'),
                required: false,
                value: item.Placeholder
              }),
              new Checkbox({
                key: 'Required',
                label: this._localization.translate('UI.LABEL.REQUIRED'),
                value: item.Required
              }),
              new Checkbox({
                key: 'ShowInGrid',
                label: this._localization.translate('UI.LABEL.SHOW_IN_GRID'),
                value: item.ShowInGrid
              }),
            ]
          }).onSuccess(modalRes => {
            this._looseData.editSetAttribute(DefaultSetId.ArrestReport, {
              Id: item.Id,
              Name: modalRes.value!['Name'],
              Label: modalRes.value!['Label'],
              HelpLabel: modalRes.value!['HelpLabel'],
              Placeholder: modalRes.value!['Placeholder'],
              Required: modalRes.value!['Required'],
              ShowInGrid: modalRes.value!['ShowInGrid'],
            }).subscribe(res => {
              if (res.Status) {
                this.getArrestReportAttributes();
              }
            });
          })
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          show: item => item.AddonId === 'admin',
          onClick: item => this._modal.open(ModalFormComponent, {
            title: 'UI.LABEL.ATTRIBUTE.DELETE.TITLE',
            description: this._localization.translate('UI.LABEL.ATTRIBUTE.DELETE.DESCRIPTION', { attribute: item.Name }),
            buttons: {
              cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
              submit: { variant: 'danger', label: 'UI.ACTION.DELETE' },
            },
          }).onSuccess(() => {
            this._looseData.deleteSetAttribute(DefaultSetId.ArrestReport, item.Id).subscribe(res => {
              if (res.Status) {
                this.arrestReportAttributesCollation.Page = 1;
                this.getArrestReportAttributes();
              }
            });
          })
        }
      ]
    }
  ];
  arrestReportAttributesButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_ATTRIBUTE'),
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this._modal.open(ModalFormComponent, {
          title: 'UI.ACTION.CREATE_ATTRIBUTE',
          buttons: {
            cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
            submit: { variant: 'primary', label: 'UI.ACTION.CREATE' },
          },
          fields: [
            new TextInput({
              key: 'Name',
              label: this._localization.translate('UI.LABEL.NAME'),
              required: true,
            }),
            new Dropdown({
              key: 'DataType',
              label: this._localization.translate('UI.LABEL.TYPE'),
              required: true,
              dropdownOptions: {
                size: 'normal',
                label: this._localization.translate('UI.LABEL.TYPE'),
                items: [
                  { key: AttributeDataType.ShortString, label: this._localization.translate('UI.LABEL.SHORT_TEXT') },
                  { key: AttributeDataType.LongString, label: this._localization.translate('UI.LABEL.LONG_TEXT') },
                  { key: AttributeDataType.Integer, label: this._localization.translate('UI.LABEL.WHOLE_NUMBER') },
                  { key: AttributeDataType.Bool, label: this._localization.translate('UI.LABEL.CHECKBOX') },
                  { key: AttributeDataType.Date, label: this._localization.translate('UI.LABEL.DATE') },
                ]
              }
            }),
            new TextInput({
              key: 'Label',
              label: this._localization.translate('UI.LABEL.LABEL'),
              required: true,
            }),
            new TextInput({
              key: 'HelpLabel',
              label: this._localization.translate('UI.LABEL.HELP_LABEL'),
              required: false,
            }),
            new TextInput({
              key: 'Placeholder',
              label: this._localization.translate('UI.LABEL.PLACEHOLDER'),
              required: false,
            }),
            new Checkbox({
              key: 'Required',
              label: this._localization.translate('UI.LABEL.REQUIRED'),
            }),
            new Checkbox({
              key: 'ShowInGrid',
              label: this._localization.translate('UI.LABEL.SHOW_IN_GRID'),
            }),
          ]
        }).onSuccess(modalRes => {
          this._looseData.addSetAttribute(DefaultSetId.ArrestReport, {
            Name: modalRes.value!['Name'],
            DataType: modalRes.value!['DataType'],
            Label: modalRes.value!['Label'],
            HelpLabel: modalRes.value!['HelpLabel'],
            Placeholder: modalRes.value!['Placeholder'],
            Required: modalRes.value!['Required'],
            ShowInGrid: modalRes.value!['ShowInGrid'],
          }).subscribe(() => {
            this.arrestReportAttributesCollation.Page = 1;
            this.getArrestReportAttributes();
          })
        })
      }
    }
  ];

  constructor(
    private _looseData: LooseDataService,
    private _modal: ModalService,
    private _localization: LocalizationService,
    private _store: Store,
  ) { }

  ngOnInit(): void {
    this._store.select(UserState).pipe(
      takeUntil(this.onDestroy$)
    ).subscribe((user: UserStateModel) => {
      this.user = user.details;
      if (user.details.Admin) {
        this.getArrestReportAttributes();
      }
    });
  }

  ngOnDestroy(): void {
    this.onDestroy$.next();
    this.onDestroy$.unsubscribe();
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
