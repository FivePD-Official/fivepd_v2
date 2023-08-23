import { Component, OnInit } from '@angular/core';
import { AttributeDataType, Button, Collation, Column, Page, SetAttribute, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { filter, first, switchMap } from 'rxjs';
import { createAttributesFormFields } from '../../../helpers/forms/set-attribute';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { LocalizationService } from '@services/localization/localization.service';
import { DefaultSetId } from '@enums';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EventService } from "@services/event/event.service";

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  protected readonly UserRole = UserRole;

  user?: User;

  arrestReportAttributes?: Page<SetAttribute>;
  arrestReportAttributesCollation: Collation = {
    Page: 1,
  };
  arrestReportAttributesColumns: Column<SetAttribute>[] = [
    {
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => true,
      accessorKey: 'Name',
    },
    {
      header: this._localization.translate('UI.LABEL.TYPE.SINGULAR'),
      show: () => true,
      valueResolver: attr => {
        switch (attr.DataType) {
          case AttributeDataType.ShortString:
            return this._localization.translate('UI.LABEL.SHORT_TEXT.SINGULAR');
          case AttributeDataType.LongString:
            return this._localization.translate('UI.LABEL.LONG_TEXT.SINGULAR');
          case AttributeDataType.Integer:
            return this._localization.translate('UI.LABEL.WHOLE_NUMBER.SINGULAR');
          case AttributeDataType.Bool:
            return this._localization.translate('UI.LABEL.CHECKBOX.SINGULAR');
          case AttributeDataType.Date:
            return this._localization.translate('UI.LABEL.DATE.SINGULAR');
          default:
            return '-';
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
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          show: item => item.AddonId === 'admin',
          onClick: attribute => {
            this._modalFormService.open({
              title: 'UI.LABEL.ATTRIBUTE.EDIT',
              buttons: {
                submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
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
                this._modalService.handleResponse()
              ).subscribe(() => this.getArrestReportAttributes())
          }
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          show: item => item.AddonId === 'admin',
          onClick: item => {
            this._modalConfirmService.open({
              title: 'UI.LABEL.ATTRIBUTE.DELETE.TITLE',
              description: this._localization.translate('UI.LABEL.ATTRIBUTE.DELETE.DESCRIPTION', {attribute: item.Name}),
              buttons: {
                submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._looseData.deleteSetAttribute(DefaultSetId.ArrestReport, item.Id)),
                this._modalService.handleResponse()
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
      label: this._localization.translate('UI.ACTION.CREATE_ATTRIBUTE'),
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this._modalFormService.open({
          title: 'UI.ACTION.CREATE_ATTRIBUTE',
          buttons: {
            submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
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
            this._modalService.handleResponse()
          ).subscribe(() => {
          this.arrestReportAttributesCollation.Page = 1;
          this.getArrestReportAttributes();
        })
      }
    }
  ];

  users?: Page<User>;
  usersCollation: Collation = {
    Page: 1,
  };
  usersColumns: Column<User>[] = [
    new Column<User>({
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => true,
      accessorKey: 'Name',
    }),
    new Column<User>({
      header: this._localization.translate('UI.LABEL.ROLE.SINGULAR'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'select',
          label: this._localization.translate('UI.LABEL.ROLE.NO'),
          selectedItemsResolver: (user, dropdownItemKey) => this.userService.userHasRole(dropdownItemKey),
          items: () => {
            return [
              {key: UserRole.Admin, label: this._localization.translate('UI.LABEL.ROLE.ADMIN')},
              {key: UserRole.Developer, label: this._localization.translate('UI.LABEL.ROLE.DEVELOPER')},
            ];
          },
          resetWhenSelectedId: UserRole.None,
          onSelectMulti: (user, selectedIds) => {
            this.userService.changeRole(user, selectedIds).subscribe();
          }
        }
      ]
    }),
  ];

  constructor(
    public readonly userService: UserService,
    private readonly _looseData: LooseDataService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _localization: LocalizationService,
    private readonly _store: Store<GlobalState>,
    private readonly _eventService: EventService,
  ) {
    _store.pipe(takeUntilDestroyed(), first()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }

  ngOnInit(): void {
    this.getSettings();
  }

  private getSettings(): void {
    if (!this.user || !this.userService.userHasRole(UserRole.Admin)) {
      return;
    }

    this.getArrestReportAttributes();
    this.getUsers();
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

  getUsers(): void {
    this.userService.getAllPageable(this.arrestReportAttributesCollation).subscribe(res => {
      this.users = res;
    });
  }

  onUsersCollationChange(collation: Collation): void {
    this.usersCollation = collation;
    this.getUsers();
  }
}
