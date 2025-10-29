import { Component, inject } from '@angular/core';
import { Button, Collation, Column, FormInputBase, ModalResponse, TextField, WeaponComponent, } from "@models";
import { BehaviorSubject, filter, map, switchMap } from "rxjs";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { GridComponent } from '../../../../../components/grid/grid.component';
import { AsyncPipe } from "@angular/common";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { LocalizationService } from "@services/localization/localization.service";
import { WeaponComponentService } from "@services/weapon-component/weapon-component.service";

function createBaseFields(weaponComponent?: WeaponComponent): FormInputBase<any>[] {
  return [
    new TextField({
      key: 'Name',
      label: 'LABEL.NAME.SINGULAR',
      helpLabel: 'LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.NAME.HELP_LABEL',
      required: true,
      value: weaponComponent?.Name,
    }),
    new TextField({
      key: 'HashKey',
      label: 'LABEL.HASH_KEY.SINGULAR', // Make localisation for HashKey
      helpLabel: 'LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.HASH_KEY.HELP_LABEL',
      required: true,
      value: weaponComponent?.HashKey,

    })
  ];
}

@Component({
    selector: 'app-loadout-weapon-components',
    templateUrl: './loadout-weapon-components.component.html',
    imports: [GridComponent, TranslatePipe, AsyncPipe]
})
export class LoadoutWeaponComponentsComponent {

  private readonly _weaponComponentService = inject(WeaponComponentService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  readonly nameColumn = new Column<WeaponComponent>({
    header: 'LABEL.NAME.SINGULAR',
    accessorKey: "Name",
    filter: {
      label: 'LABEL.SEARCH_BY.NAME',
    }
  });
  readonly hashKeyColumn = new Column<WeaponComponent>({
    header: 'LABEL.HASH_KEY.SINGULAR',
    accessorKey: "HashKey",
    filter: {
      label: 'LABEL.SEARCH_BY.HASH_KEY',
    }
  });

  readonly weaponComponentCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly weaponComponentPage$ = this.weaponComponentCollation$.pipe(
    switchMap(collation => this._weaponComponentService.getPage(collation))
  );
  readonly weaponComponentButtons: Button[] = [
    this.createAddWeaponComponentButton(
      createBaseFields,
      () => this.weaponComponentCollation$.next(this.weaponComponentCollation$.value)
    ),
  ];
  readonly weaponComponentColumns: Column<WeaponComponent>[] = [
    this.nameColumn,
    this.hashKeyColumn,
    this.createActionsColumn(createBaseFields, () => this.weaponComponentCollation$.next(this.weaponComponentCollation$.value)),
  ];

  private createActionsColumn(fields: (weaponComponent: WeaponComponent | undefined) => FormInputBase<any>[], onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Column<WeaponComponent> {
    return new Column<WeaponComponent>({
      header: 'LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'ACTION.EDIT.TITLE',
          show: () => true,
          disabled: () => false,
          onClick: weaponComponent => {
            this._modalFormService.open({
              title: 'LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.EDIT.TITLE',
              fields: weaponComponent => fields(weaponComponent),
              valueResolver: new ValueResolver(weaponComponent, this._weaponComponentService.get(weaponComponent.Id)),
              buttons: {
                submit: {
                  variant: 'primary',
                  label: 'ACTION.SAVE.TITLE',
                },
              },
            }).result
              .pipe(
                filter(res => res.success),
                map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
                switchMap(res => this._weaponComponentService.edit({
                  ...res.value,
                  Id: weaponComponent.Id,
                })),
                this._modalService.handleResponse(),
              ).subscribe(onResponse)
          }
        },
        {
          type: 'button',
          label: 'ACTION.DELETE.TITLE',
          show: () => true,
          disabled: () => false,
          onClick: weaponComponent => this._modalConfirmService.open({
            title: 'LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.DELETE.TITLE',
            description: this._localizationService.translate('LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.DELETE.DESCRIPTION', {name: weaponComponent.Name}),
            buttons: {
              submit: {
                variant: 'danger',
                label: 'ACTION.DELETE.TITLE',
              },
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._weaponComponentService.delete(weaponComponent.Id)),
              this._modalService.handleResponse(),
            ).subscribe(onResponse)
        },
      ]
    });
  }

  private createAddWeaponComponentButton(fields: () => FormInputBase<any>[], onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Button {
    return {
      label: this._localizationService.translate('LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.CREATE.TITLE'),
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.CREATE.TITLE',
        fields,
        buttons: {
          submit: {
            variant: 'primary',
            label: 'ACTION.CREATE.TITLE',
          },
        },
      }).result
        .pipe(
          filter(res => res.success),
          map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
          switchMap(res => this._weaponComponentService.create({
            ...res.value,
          })),
          this._modalService.handleResponse(),
        ).subscribe(onResponse)
    };
  }
}
