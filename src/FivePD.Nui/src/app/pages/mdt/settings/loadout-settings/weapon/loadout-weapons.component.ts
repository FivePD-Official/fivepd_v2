import {Component, inject} from '@angular/core';
import {
  Button,
  Collation,
  Column, ModalResponse,
  MultiSelectField,
  SimpleWeaponComponent,
  TextField,
  Weapon
} from "@models";
import {BehaviorSubject, filter, map, switchMap} from "rxjs";
import {ModalService} from "@services/modals/modal/modal.service";
import {ModalFormService} from "@services/modals/modal-form/modal-form.service";
import {ModalConfirmService} from "@services/modals/modal-confirm/modal-confirm.service";
import {TranslatePipe} from "../../../../../pipes/translate.pipe";
import {GridComponent} from '../../../../../components/grid/grid.component';
import {ContainerComponent} from '../../../../../components/container/container.component';
import {PageHeaderComponent} from '../../../../../components/page-header/page-header.component';
import {
  SingleSelectFieldComponent
} from "../../../../../components/form-inputs/single-select-field/single-select-field.component";
import {AsyncPipe} from "@angular/common";
import {ValueResolver} from "@services/modals/modal-form/modal-form-value-resolver";
import {LocalizationService} from "@services/localization/localization.service";
import {WeaponService} from "@services/weapon/weapon.service";
import {WeaponComponentService} from "@services/weapon-component/weapon-component.service";
import {DataResolver} from "@services/modals/modal-form/modal-form-data-resolver";

@Component({
  selector: 'app-loadout-weapons',
  templateUrl: './loadout-weapons.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent, AsyncPipe]
})
export class LoadoutWeaponsComponent {

  private readonly _weaponService = inject(WeaponService);
  private readonly _weaponComponentService = inject(WeaponComponentService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  readonly weaponCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly weaponPage$ = this.weaponCollation$.pipe(
    switchMap(collation => this._weaponService.getPage(collation))
  );
  readonly weaponButtons: Button[] = [
    this.createAddWeaponButton(
      () => this.weaponCollation$.next(this.weaponCollation$.value)
    ),
  ];

  readonly nameColumn = new Column<Weapon>({
    header: 'UI.LABEL.NAME.SINGULAR',
    accessorKey: "Name",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.NAME',
    }
  });
  readonly modelColumn = new Column<Weapon>({
    header: 'UI.LABEL.MODEL.SINGULAR',
    accessorKey: "Model",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.MODEL',
    }
  });

  readonly weaponColumns: Column<Weapon>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(() => this.weaponCollation$.next(this.weaponCollation$.value)),
  ];

  private createActionsColumn(onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Column<Weapon> {
    return new Column<Weapon>({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => true,
          disabled: () => false,
          onClick: weapon => {
            this._modalFormService.open({
              title: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.EDIT.TITLE',
              fields: (weapon, wepComponents) => {
                const weaponComponents: SimpleWeaponComponent[] = Array.isArray(wepComponents) ? wepComponents : [];
                return [
                  new TextField({
                    key: 'Name',
                    label: 'UI.LABEL.NAME.SINGULAR',
                    helpLabel: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.NAME.HELP_LABEL',
                    required: true,
                    value: weapon?.Name,
                  }),
                  new TextField({
                    key: 'Model',
                    label: 'UI.LABEL.MODEL.SINGULAR',
                    helpLabel: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.MODEL.HELP_LABEL',
                    required: true,
                    value: weapon?.Model,
                  }),
                  new MultiSelectField({
                    key: 'WeaponComponentIds',
                    label: 'UI.LABEL.WEAPON_COMPONENT.PLURAL',
                    defaultItemLabel: '',
                    addDefaultItem: false,
                    value: weapon?.WeaponComponentIds,
                    items: weaponComponents.map(wc => ({
                      key: wc.Id,
                      label: wc.Name,
                    })),
                    size: "normal",
                    height: "base",
                    variant: "secondary",
                    required: false,
                  })
                ]
              },
              valueResolver: new ValueResolver(weapon, this._weaponService.get(weapon.Id)),
              dataResolver: new DataResolver([], this._weaponComponentService.getAllSimple()),
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
                switchMap(res => this._weaponService.edit({
                  ...res.value,
                  Id: weapon.Id,
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
          onClick: weapon => this._modalConfirmService.open({
            title: 'UI.LABEL.LOADOUT_CONFIG.DELETE.TITLE',
            description: this._localizationService.translate('UI.LABEL.LOADOUT_CONFIG.WEAPON.DELETE.DESCRIPTION', {name: weapon.Name}),
            buttons: {
              submit: {
                variant: 'danger',
                label: 'UI.ACTION.DELETE',
              },
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._weaponService.delete(weapon.Id)),
              this._modalService.handleResponse(),
            ).subscribe(onResponse)
        },
      ]
    });
  }

  private createAddWeaponButton(onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Button {
    return {
      label: this._localizationService.translate('UI.LABEL.LOADOUT_CONFIG.WEAPON.CREATE.TITLE'),
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.CREATE.TITLE',
        fields: (wepComponents) => {
          const weaponComponents: SimpleWeaponComponent[] = Array.isArray(wepComponents) ? wepComponents : [];
          return [
            new TextField({
              key: 'Name',
              label: 'UI.LABEL.NAME.SINGULAR',
              helpLabel: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.NAME.HELP_LABEL',
              required: true,
            }),
            new TextField({
              key: 'Model',
              label: 'UI.LABEL.MODEL.SINGULAR',
              helpLabel: 'UI.LABEL.LOADOUT_CONFIG.WEAPON.MODEL.HELP_LABEL',
              required: true,
            }),
            new MultiSelectField({
              key: 'WeaponComponentIds',
              label: 'UI.LABEL.WEAPON_COMPONENT.PLURAL',
              defaultItemLabel: '',
              addDefaultItem: false,
              items: weaponComponents.map(wc => ({
                key: wc.Id,
                label: wc.Name,
              })),
              size: "normal",
              height: "base",
              variant: "secondary",
              required: false
            })
          ];
        },
        buttons: {
          submit: {
            variant: 'primary',
            label: 'UI.ACTION.CREATE',
          },
        },
        valueResolver: new ValueResolver([], this._weaponComponentService.getAllSimple()),
      }).result
        .pipe(
          filter(res => res.success),
          map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
          switchMap(res => this._weaponService.isWeaponValid({
            ...res.value,
          })),
          this._modalService.handleResponse(),
        ).subscribe(onResponse)
    };
  }
}
