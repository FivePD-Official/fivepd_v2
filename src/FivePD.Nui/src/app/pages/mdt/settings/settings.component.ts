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
import { SetId } from '@enums';
import { ModalService } from '@services/modal/modal.service';
import { UserState } from '@store/user.state';
import { Observable } from 'rxjs';
import { Select } from '@ngxs/store';
import { LocalizationService } from '@services/localization/localization.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  user?: User;
  @Select(UserState.details) user$!: Observable<User>;

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
          case AttributeDataType.String: return this._localization.translate('Nui.Labels.String');
          case AttributeDataType.Integer: return this._localization.translate('Nui.Labels.Integer');
          case AttributeDataType.Bool: return this._localization.translate('Nui.Labels.Boolean');
          case AttributeDataType.DateTime: return this._localization.translate('Nui.Labels.DateTime');
          default: return '-';
        }
      },
    },
    {
      header: this._localization.translate('Nui.Labels.Required'),
      show: () => true,
      accessorKey: 'Required',
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
          onClick: item => this._modal.open({
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
                    { key: AttributeDataType.String, label: this._localization.translate('Nui.Labels.String') },
                    { key: AttributeDataType.Integer, label: this._localization.translate('Nui.Labels.Integer') },
                    { key: AttributeDataType.Bool, label: this._localization.translate('Nui.Labels.Boolean') },
                    { key: AttributeDataType.DateTime, label: this._localization.translate('Nui.Labels.DateTime') },
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
                required: true,
                value: item.HelpLabel
              }),
              new TextInput({
                key: 'Placeholder',
                label: this._localization.translate('Nui.Labels.Placeholder'),
                required: true,
                value: item.Placeholder
              }),
              new Checkbox({
                key: 'Required',
                label: this._localization.translate('Nui.Labels.Required'),
                value: item.Required
              }),
            ]
          }).onSuccess(modalRes => {
            this._looseData.editSetAttribute(SetId.ArrestReport, {
              Id: item.Id,
              Name: modalRes.value!['Name'],
              Label: modalRes.value!['Label'],
              HelpLabel: modalRes.value!['HelpLabel'],
              Placeholder: modalRes.value!['Placeholder'],
              Required: modalRes.value!['Required'],
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
          onClick: item => this._modal.open({
            title: 'Nui.Labels.DeleteAttributeTitle',
            description: this._localization.translate('Nui.Labels.DeleteAttributeDescription', { attribute: item.Name }),
            buttons: {
              cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
              submit: { theme: 'danger', label: 'Nui.Actions.Delete' },
            },
          }).onSuccess(() => {
            this._looseData.deleteSetAttribute(SetId.ArrestReport, item.Id).subscribe(res => {
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
      onClick: () => {
        this._modal.open({
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
                  { key: AttributeDataType.String, label: this._localization.translate('Nui.Labels.String') },
                  { key: AttributeDataType.Integer, label: this._localization.translate('Nui.Labels.Integer') },
                  { key: AttributeDataType.Bool, label: this._localization.translate('Nui.Labels.Boolean') },
                  { key: AttributeDataType.DateTime, label: this._localization.translate('Nui.Labels.DateTime') },
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
              required: true,
            }),
            new TextInput({
              key: 'Placeholder',
              label: this._localization.translate('Nui.Labels.Placeholder'),
              required: true,
            }),
            new Checkbox({
              key: 'Required',
              label: this._localization.translate('Nui.Labels.Required'),
            }),
          ]
        }).onSuccess(modalRes => {
          this._looseData.addSetAttribute(SetId.ArrestReport, {
            Name: modalRes.value!['Name'],
            DataType: modalRes.value!['DataType'],
            Label: modalRes.value!['Label'],
            HelpLabel: modalRes.value!['HelpLabel'],
            Placeholder: modalRes.value!['Placeholder'],
            Required: modalRes.value!['Required'],
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
  ) { }

  ngOnInit(): void {
    this.user$.subscribe(user => {
      this.user = user;
      if (user.Admin) {
        this.getArrestReportAttributes();
      }
    });
  }

  getArrestReportAttributes(): void {
    this._looseData.getSetAttributesPage(SetId.ArrestReport, this.arrestReportAttributesCollation).subscribe(res => {
      this.arrestReportAttributes = res;
    });
  }

  onArrestReportAttributesCollationChange(collation: Collation): void {
    this.arrestReportAttributesCollation = collation;
    this.getArrestReportAttributes();
  }
}
