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
      header: this._localization.translate('Nui.Labels.Name'),
      show: () => true,
      accessorKey: 'Name',
    },
    {
      header: this._localization.translate('Nui.Labels.Type'),
      show: () => true,
      valueResolver: attr => {
        switch (attr.DataType) {
          case AttributeDataType.ShortString: return this._localization.translate('Nui.Labels.ShortString');
          case AttributeDataType.LongString: return this._localization.translate('Nui.Labels.LongString');
          case AttributeDataType.Integer: return this._localization.translate('Nui.Labels.WholeNumber');
          case AttributeDataType.Bool: return this._localization.translate('Nui.Labels.Checkbox');
          case AttributeDataType.Date: return this._localization.translate('Nui.Labels.Date');
          default: return '-';
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
          onClick: item => this._modal.open(ModalFormComponent, {
            title: 'Nui.Labels.EditAttributeTitle',
            buttons: {
              cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
              submit: { theme: 'primary', label: 'Nui.Actions.Save' },
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
                  label: this._localization.translate('Nui.Labels.Type'),
                  selectedItemResolver: itemKey => item.DataType === itemKey,
                  items: [
                    { key: AttributeDataType.ShortString, label: this._localization.translate('Nui.Labels.ShortString') },
                    { key: AttributeDataType.LongString, label: this._localization.translate('Nui.Labels.LongString') },
                    { key: AttributeDataType.Integer, label: this._localization.translate('Nui.Labels.WholeNumber') },
                    { key: AttributeDataType.Bool, label: this._localization.translate('Nui.Labels.Checkbox') },
                    { key: AttributeDataType.Date, label: this._localization.translate('Nui.Labels.Date') },
                  ]
                }
              }),
              new TextInput({
                key: 'Label',
                label: this._localization.translate('Nui.Labels.Label'),
                required: true,
                value: item.Label
              }),
              new TextInput({
                key: 'HelpLabel',
                label: this._localization.translate('Nui.Labels.HelpLabel'),
                required: false,
                value: item.HelpLabel
              }),
              new TextInput({
                key: 'Placeholder',
                label: this._localization.translate('Nui.Labels.Placeholder'),
                required: false,
                value: item.Placeholder
              }),
              new Checkbox({
                key: 'Required',
                label: this._localization.translate('Nui.Labels.Required'),
                value: item.Required
              }),
              new Checkbox({
                key: 'ShowInGrid',
                label: this._localization.translate('Nui.Labels.ShowInGrid'),
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
          label: this._localization.translate('Nui.Actions.Delete'),
          show: item => item.AddonId === 'admin',
          onClick: item => this._modal.open(ModalFormComponent, {
            title: 'Nui.Labels.DeleteAttributeTitle',
            description: this._localization.translate('Nui.Labels.DeleteAttributeDescription', { attribute: item.Name }),
            buttons: {
              cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
              submit: { theme: 'danger', label: 'Nui.Actions.Delete' },
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
      label: this._localization.translate('Nui.Actions.CreateAttribute'),
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this._modal.open(ModalFormComponent, {
          title: 'Nui.Actions.CreateAttribute',
          buttons: {
            cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
            submit: { theme: 'primary', label: 'Nui.Actions.Create' },
          },
          fields: [
            new TextInput({
              key: 'Name',
              label: this._localization.translate('Nui.Labels.Name'),
              required: true,
            }),
            new Dropdown({
              key: 'DataType',
              label: this._localization.translate('Nui.Labels.Type'),
              required: true,
              dropdownOptions: {
                size: 'normal',
                label: this._localization.translate('Nui.Labels.Type'),
                items: [
                  { key: AttributeDataType.ShortString, label: this._localization.translate('Nui.Labels.ShortString') },
                  { key: AttributeDataType.LongString, label: this._localization.translate('Nui.Labels.LongString') },
                  { key: AttributeDataType.Integer, label: this._localization.translate('Nui.Labels.WholeNumber') },
                  { key: AttributeDataType.Bool, label: this._localization.translate('Nui.Labels.Checkbox') },
                  { key: AttributeDataType.Date, label: this._localization.translate('Nui.Labels.Date') },
                ]
              }
            }),
            new TextInput({
              key: 'Label',
              label: this._localization.translate('Nui.Labels.Label'),
              required: true,
            }),
            new TextInput({
              key: 'HelpLabel',
              label: this._localization.translate('Nui.Labels.HelpLabel'),
              required: false,
            }),
            new TextInput({
              key: 'Placeholder',
              label: this._localization.translate('Nui.Labels.Placeholder'),
              required: false,
            }),
            new Checkbox({
              key: 'Required',
              label: this._localization.translate('Nui.Labels.Required'),
            }),
            new Checkbox({
              key: 'ShowInGrid',
              label: this._localization.translate('Nui.Labels.ShowInGrid'),
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
