import {Component, inject} from '@angular/core';
import {
  Button,
  Collation,
  Column,
  LoadoutConfig,
  ModalResponse,
  MultiSelectField,
  SimpleWeapon,
  TextField
} from "@models";
import {BehaviorSubject, filter, map, switchMap} from "rxjs";
import {UserRole, UserService} from "@services/user/user.service";
import {ModalService} from "@services/modals/modal/modal.service";
import {ModalFormService} from "@services/modals/modal-form/modal-form.service";
import {ModalConfirmService} from "@services/modals/modal-confirm/modal-confirm.service";
import {TranslatePipe} from "../../../../pipes/translate.pipe";
import {GridComponent} from '../../../../components/grid/grid.component';
import {ContainerComponent} from '../../../../components/container/container.component';
import {PageHeaderComponent} from '../../../../components/page-header/page-header.component';
import {
  SingleSelectFieldComponent
} from "../../../../components/form-inputs/single-select-field/single-select-field.component";
import {AsyncPipe} from "@angular/common";
import {ValueResolver} from "@services/modals/modal-form/modal-form-value-resolver";
import {LocalizationService} from "@services/localization/localization.service";
import {LoadoutConfigService} from "@services/loadout-config/loadout-config.service";
import {LoadoutWeaponsComponent} from "./weapon/loadout-weapons.component";
import {LoadoutWeaponComponentsComponent} from "./weapon-components/loadout-weapon-components.component";
import {DataResolver} from "@services/modals/modal-form/modal-form-data-resolver";
import {WeaponService} from "@services/weapon/weapon.service";

@Component({
  selector: 'app-loadout-settings',
  templateUrl: './loadout-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent, AsyncPipe, LoadoutWeaponsComponent, LoadoutWeaponComponentsComponent]
})
export class LoadoutSettingsComponent {

  protected readonly UserRole = UserRole;

  protected readonly _userService = inject(UserService);
  private readonly _loadoutConfigService = inject(LoadoutConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _weaponService = inject(WeaponService);

  readonly nameColumn = new Column<LoadoutConfig>({
    header: 'UI.LABEL.NAME.SINGULAR',
    accessorKey: "Name",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.NAME',
    }
  });

  readonly loadoutConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly loadoutConfigPage$ = this.loadoutConfigCollation$.pipe(
    switchMap(collation => this._loadoutConfigService.getPage(collation))
  );
  readonly loadoutConfigButtons: Button[] = [
    this.createAddLoadoutButton(
      () => this.loadoutConfigCollation$.next(this.loadoutConfigCollation$.value)
    ),
  ];
  readonly loadoutConfigColumns: Column<LoadoutConfig>[] = [
    this.nameColumn,
    this.createActionsColumn(() => this.loadoutConfigCollation$.next(this.loadoutConfigCollation$.value)),
  ];

  private createActionsColumn(onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Column<LoadoutConfig> {
    return new Column<LoadoutConfig>({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => true,
          disabled: () => false,
          onClick: loadoutConfig => {
            this._modalFormService.open({
              title: 'UI.LABEL.LOADOUT_CONFIG.EDIT.TITLE',
              fields: (loadoutConfig, weps) => {
                const weapons: SimpleWeapon[] = Array.isArray(weps) ? weps : [];
                return [
                  new TextField({
                    key: 'Name',
                    label: 'UI.LABEL.NAME.SINGULAR',
                    helpLabel: 'UI.LABEL.LOADOUT_CONFIG.NAME.HELP_LABEL',
                    required: true,
                    value: loadoutConfig?.Name,
                  }),
                  new MultiSelectField({
                    key: 'WeaponIds',
                    label: 'UI.LABEL.WEAPON.PLURAL',
                    defaultItemLabel: '',
                    addDefaultItem: false,
                    value: loadoutConfig?.WeaponIds,
                    items: weapons.map(w => ({
                      key: w.Id,
                      label: w.Name,
                    })),
                    size: "normal",
                    height: "base",
                    variant: "secondary",
                    required: false
                  })
                ]
              },
              valueResolver: new ValueResolver(loadoutConfig, this._loadoutConfigService.get(loadoutConfig.Id)),
              dataResolver: new DataResolver([], this._weaponService.getAllSimple()),
              buttons: {
                submit: {
                  variant: 'primary',
                  label: 'UI.ACTION.SAVE',
                },
              },
            }).result
              .pipe(
                filter(res => res.success),
                map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
                switchMap(res => this._loadoutConfigService.edit({
                  ...res.value,
                  Id: loadoutConfig.Id,
                })),
                this._modalService.handleResponse(),
              ).subscribe(onResponse)
          }
        },
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => true,
          disabled: () => false,
          onClick: loadoutConfig => this._modalConfirmService.open({
            title: 'UI.LABEL.LOADOUT_CONFIG.DELETE.TITLE',
            description: this._localizationService.translate('UI.LABEL.LOADOUT_CONFIG.DELETE.DESCRIPTION', {name: loadoutConfig.Name}),
            buttons: {
              submit: {
                variant: 'danger',
                label: 'UI.ACTION.DELETE',
              },
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._loadoutConfigService.delete(loadoutConfig.Id)),
              this._modalService.handleResponse(),
            ).subscribe(onResponse)
        },
      ]
    });
  }

  private createAddLoadoutButton(onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Button {
    return {
      label: this._localizationService.translate('UI.LABEL.LOADOUT_CONFIG.CREATE.TITLE'),
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'UI.LABEL.LOADOUT_CONFIG.CREATE.TITLE',
        fields: (undefined, weps) => {
          const weapons: SimpleWeapon[] = Array.isArray(weps) ? weps : [];
          return [
            new TextField({
              key: 'Name',
              label: 'UI.LABEL.NAME.SINGULAR',
              helpLabel: 'UI.LABEL.LOADOUT_CONFIG.NAME.HELP_LABEL',
              required: true,
            }),
            new MultiSelectField({
              key: 'WeaponIds',
              label: 'UI.LABEL.WEAPON.PLURAL',
              defaultItemLabel: '',
              addDefaultItem: false,
              items: weapons.map(w => ({
                key: w.Id,
                label: w.Name,
              })),
              size: "normal",
              height: "base",
              variant: "secondary",
              required: false
            })
          ]
        },
        dataResolver: new DataResolver([], this._weaponService.getAllSimple()),
        buttons: {
          submit: {
            variant: 'primary',
            label: 'UI.ACTION.CREATE',
          },
        },
      }).result
        .pipe(
          filter(res => res.success),
          map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
          switchMap(res => this._loadoutConfigService.create({
            ...res.value,
          })),
          this._modalService.handleResponse(),
        ).subscribe(onResponse)
    };
  }
}
