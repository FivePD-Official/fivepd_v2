import {AttributeDataType, Button, Column, VariableDataAttribute} from "@models";
import {createAttributesFormFields} from "../../../../helpers/forms/set-attribute";
import {ValueResolver} from "@services/modals/modal-form/modal-form-value-resolver";
import {filter, switchMap} from "rxjs";
import {UserRole, UserService} from "@services/user/user.service";
import {LocalizationService} from "@services/localization/localization.service";
import {ModalFormService} from "@services/modals/modal-form/modal-form.service";
import {inject, Injectable} from "@angular/core";
import {ModalService} from "@services/modals/modal/modal.service";
import {ModalConfirmService} from "@services/modals/modal-confirm/modal-confirm.service";
import { VariableDataService } from "@services/variable-data/variable-data.service";
import { BadgeVariants } from "../../../../components/badge/badge.component";

@Injectable({
  providedIn: 'root'
})
export class AttributeGridOptions {

  private readonly _userService = inject(UserService);
  private readonly _variableDataService = inject(VariableDataService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  valueResolver = (item: VariableDataAttribute, key: keyof VariableDataAttribute): string => this._localizationService.translate(item[key] ? 'LABEL.YES.TITLE' : 'UI.LABEL.NO');
  badgeResolver = (item: VariableDataAttribute, key: keyof VariableDataAttribute): BadgeVariants => item[key] ? 'green' : 'gray';

  createColumnList(set: string, onEdit: () => void, onDelete: () => void): Column<VariableDataAttribute>[] {
    return [
      {
        header: this._localizationService.translate('LABEL.NAME.SINGULAR'),
        show: () => true,
        accessorKey: 'Name',
      },
      {
        header: this._localizationService.translate('LABEL.TYPE.SINGULAR'),
        show: () => true,
        valueResolver: attr => {
          switch (attr.DataType) {
            case AttributeDataType.ShortString:
              return this._localizationService.translate('LABEL.SHORT_TEXT.SINGULAR');
            case AttributeDataType.LongString:
              return this._localizationService.translate('LABEL.LONG_TEXT.SINGULAR');
            case AttributeDataType.Integer:
              return this._localizationService.translate('LABEL.WHOLE_NUMBER.SINGULAR');
            case AttributeDataType.Bool:
              return this._localizationService.translate('LABEL.CHECKBOX.SINGULAR');
            case AttributeDataType.Date:
              return this._localizationService.translate('LABEL.DATE.TITLE.SINGULAR');
            case AttributeDataType.IntegerList:
              return this._localizationService.translate('LABEL.WHOLE_NUMBER_LIST.SINGULAR');
            default:
              return '-';
          }
        },
      },
      {
        header: this._localizationService.translate('LABEL.HIDDEN.TITLE'),
        show: () => true,
        valueResolver: item => this.valueResolver(item, 'Hidden'),
        badgeResolver: item => this.badgeResolver(item, 'Hidden'),
      },
      {
        header: this._localizationService.translate('LABEL.READONLY.TITLE'),
        show: () => true,
        valueResolver: item => this.valueResolver(item, 'Readonly'),
        badgeResolver: item => this.badgeResolver(item, 'Readonly'),
      },
      {
        header: this._localizationService.translate('LABEL.REQUIRED.TITLE'),
        show: () => true,
        valueResolver: item => this.valueResolver(item, 'Required'),
        badgeResolver: item => this.badgeResolver(item, 'Required'),
      },
      {
        header: this._localizationService.translate('LABEL.SHOW_IN_GRID.TITLE'),
        show: () => true,
        valueResolver: item => this.valueResolver(item, 'ShowInGrid'),
        badgeResolver: item => this.badgeResolver(item, 'ShowInGrid'),
      },
      {
        header: this._localizationService.translate('LABEL.ADDED_BY'),
        show: () => true,
        accessorKey: 'AddonId',
      },
      {
        header: this._localizationService.translate('LABEL.ACTION.PLURAL'),
        show: () => this._userService.userHasRole(UserRole.Admin),
        actions: [
          {
            type: 'button',
            label: this._localizationService.translate('ACTION.EDIT.TITLE'),
            onClick: attribute => {
              this._modalFormService.open({
                title: 'LABEL.ATTRIBUTE.EDIT',
                buttons: {
                  submit: {variant: 'primary', label: 'ACTION.SAVE.TITLE'},
                },
                fields: updatedAttribute => createAttributesFormFields(this._localizationService, updatedAttribute),
                valueResolver: new ValueResolver<VariableDataAttribute>(attribute, this._variableDataService.getSetAttribute(set, attribute.Id))
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(res => this._variableDataService.editSetAttribute(set, {
                    Id: attribute.Id,
                    Name: res.value['Name'],
                    Label: res.value['Label'],
                    HelpLabel: res.value['HelpLabel'],
                    Placeholder: res.value['Placeholder'],
                    Hidden: res.value['Hidden'],
                    Readonly: res.value['Readonly'],
                    Required: res.value['Required'],
                    ShowInGrid: res.value['ShowInGrid'],
                  })),
                  this._modalService.handleResponse()
                ).subscribe(() => onEdit())
            }
          },
          {
            type: 'button',
            label: this._localizationService.translate('ACTION.DELETE.TITLE'),
            onClick: item => {
              this._modalConfirmService.open({
                title: 'LABEL.ATTRIBUTE.DELETE.TITLE',
                description: this._localizationService.translate('LABEL.ATTRIBUTE.DELETE.DESCRIPTION', {
                  attribute: item.Name,
                  set
                }),
                buttons: {
                  submit: {variant: 'danger', label: 'ACTION.DELETE.TITLE'},
                },
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(() => this._variableDataService.deleteSetAttribute(set, item.Id)),
                  this._modalService.handleResponse()
                ).subscribe(() => onDelete())
            }
          }
        ]
      }
    ];
  }

  createButtons(set: string, onCreate: () => void): Button[] {
    return [
      {
        label: this._localizationService.translate('LABEL.ATTRIBUTE.CREATE.TITLE'),
        show: () => true,
        disabled: () => false,
        onClick: () => {
          this._modalFormService.open({
            title: 'LABEL.ATTRIBUTE.CREATE.TITLE',
            description: this._localizationService.translate('LABEL.ATTRIBUTE.TITLE.DESCRIPTION', {set}, false),
            columns: 2,
            buttons: {
              submit: {variant: 'primary', label: 'ACTION.CREATE.TITLE'},
            },
            fields: () => createAttributesFormFields(this._localizationService)
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(res => this._variableDataService.addSetAttribute(
                set,
                res.value['AttributeType'] === -1 ? '' : res.value['AttributeType'],
                {
                  Name: res.value['Name'],
                  DataType: res.value['DataType'],
                  Label: res.value['Label'],
                  HelpLabel: res.value['HelpLabel'],
                  Placeholder: res.value['Placeholder'],
                  Hidden: res.value['Hidden'],
                  Readonly: res.value['Readonly'],
                  Required: res.value['Required'],
                  ShowInGrid: res.value['ShowInGrid'],
                })),
              this._modalService.handleResponse()
            ).subscribe(() => onCreate())
        }
      }
    ];
  }

}
