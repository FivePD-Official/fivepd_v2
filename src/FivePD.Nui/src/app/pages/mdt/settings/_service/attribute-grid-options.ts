import { AttributeDataType, Button, Column, LooseDataAttribute } from "@models";
import { createAttributesFormFields } from "../../../../helpers/forms/set-attribute";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { filter, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LocalizationService } from "@services/localization/localization.service";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { inject, Injectable } from "@angular/core";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";

@Injectable({
  providedIn: 'root'
})
export class AttributeGridOptions {

  private readonly _userService = inject(UserService);
  private readonly _looseDataService = inject(LooseDataService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  createColumnList(set: string, onEdit: () => void, onDelete: () => void): Column<LooseDataAttribute>[] {
    return [
      {
        header: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
        show: () => true,
        accessorKey: 'Name',
      },
      {
        header: this._localizationService.translate('UI.LABEL.TYPE.SINGULAR'),
        show: () => true,
        valueResolver: attr => {
          switch (attr.DataType) {
            case AttributeDataType.ShortString:
              return this._localizationService.translate('UI.LABEL.SHORT_TEXT.SINGULAR');
            case AttributeDataType.LongString:
              return this._localizationService.translate('UI.LABEL.LONG_TEXT.SINGULAR');
            case AttributeDataType.Integer:
              return this._localizationService.translate('UI.LABEL.WHOLE_NUMBER.SINGULAR');
            case AttributeDataType.Bool:
              return this._localizationService.translate('UI.LABEL.CHECKBOX.SINGULAR');
            case AttributeDataType.Date:
              return this._localizationService.translate('UI.LABEL.DATE.SINGULAR');
            case AttributeDataType.IntegerList:
              return this._localizationService.translate('UI.LABEL.WHOLE_NUMBER_LIST.SINGULAR');
            default:
              return '-';
          }
        },
      },
      {
        header: this._localizationService.translate('UI.LABEL.REQUIRED'),
        show: () => true,
        valueResolver: item => this._localizationService.translate(item.Required ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      },
      {
        header: this._localizationService.translate('UI.LABEL.ADDED_BY'),
        show: () => true,
        accessorKey: 'AddonId',
      },
      {
        header: this._localizationService.translate('UI.LABEL.ACTION.PLURAL'),
        show: () => this._userService.userHasRole(UserRole.Admin),
        actions: [
          {
            type: 'button',
            label: this._localizationService.translate('UI.ACTION.EDIT'),
            onClick: attribute => {
              this._modalFormService.open({
                title: 'UI.LABEL.ATTRIBUTE.EDIT',
                buttons: {
                  submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
                },
                fields: updatedAttribute => createAttributesFormFields(this._localizationService, updatedAttribute),
                valueResolver: new ValueResolver<LooseDataAttribute>(attribute, this._looseDataService.getSetAttribute(set, attribute.Id))
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(res => this._looseDataService.editSetAttribute(set, {
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
            label: this._localizationService.translate('UI.ACTION.DELETE'),
            onClick: item => {
              this._modalConfirmService.open({
                title: 'UI.LABEL.ATTRIBUTE.DELETE.TITLE',
                description: this._localizationService.translate('UI.LABEL.ATTRIBUTE.DELETE.DESCRIPTION', {attribute: item.Name, set}),
                buttons: {
                  submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
                },
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(() => this._looseDataService.deleteSetAttribute(set, item.Id)),
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
        label: this._localizationService.translate('UI.ACTION.CREATE_ATTRIBUTE.TITLE'),
        show: () => true,
        disabled: () => false,
        onClick: () => {
          this._modalFormService.open({
            title: 'UI.ACTION.CREATE_ATTRIBUTE.TITLE',
            description: this._localizationService.translate('UI.ACTION.CREATE_ATTRIBUTE.DESCRIPTION', {set}, false),
            columns: 2,
            buttons: {
              submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
            },
            fields: () => createAttributesFormFields(this._localizationService)
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(res => this._looseDataService.addSetAttribute(
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
