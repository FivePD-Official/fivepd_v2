import { inject, Injectable } from "@angular/core";
import { RankService } from "@services/rank/rank.service";
import {
  Button,
  CheckboxField,
  Column,
  FormInputBase,
  GeneralConfig,
  ModalResponse,
  NumberField,
  Page,
  Rank,
  TextField
} from "@models";
import { filter, switchMap } from "rxjs";
import { LocalizationService } from "@services/localization/localization.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";

export interface FormattedRank extends Rank {
  Permissions: string;
}

@Injectable({
  providedIn: 'root'
})
export class RankOptions {

  private readonly _localizationService = inject(LocalizationService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _rankService = inject(RankService);

  createColumns(options: {
    generalConfig: GeneralConfig;
    showActionsColumn: boolean;
    onEditSuccess: (response: ModalResponse, rank: FormattedRank) => void;
    onDeleteSuccess: () => void;
  }): Column<FormattedRank>[] {
    const columns: Column<FormattedRank>[] = [
      new Column({
        header: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
        accessorKey: 'Name',
        filter: {
          label: this._localizationService.translate('UI.LABEL.SEARCH_BY.NAME'),
        },
      }),
      new Column({
        header: this._localizationService.translate('UI.LABEL.PERMISSIONS'),
        accessorKey: 'Permissions',
      }),
    ];

    if (options.generalConfig.config.Features.ProgressionSystem) {
      columns.push(new Column({
        header: this._localizationService.translate('UI.LABEL.REQUIRED_XP'),
        accessorKey: 'RequiredXp',
      }));
    }

    if (options.showActionsColumn) {
      columns.push(new Column({
        header: this._localizationService.translate('UI.LABEL.ACTION.PLURAL'),
        actions: [
          {
            type: 'button',
            label: this._localizationService.translate('UI.ACTION.EDIT'),
            onClick: rank => {
              this._modalFormService.open<FormattedRank>({
                title: 'UI.LABEL.RANK.EDIT',
                fields: updatedRank => {
                  const fields: FormInputBase<any>[] = [
                    new TextField({
                      key: 'Name',
                      label: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
                      placeholder: this._localizationService.translate('UI.LABEL.NAME.SINGULAR'),
                      required: true,
                      value: updatedRank?.Name,
                    }),
                  ];

                  if (options.generalConfig.config.Features.ProgressionSystem) {
                    fields.push(new NumberField({
                      key: 'RequiredXp',
                      label: this._localizationService.translate('UI.LABEL.REQUIRED_XP'),
                      placeholder: this._localizationService.translate('UI.LABEL.REQUIRED_XP'),
                      required: true,
                      value: updatedRank?.RequiredXp,
                    }))
                  }

                  fields.push(new CheckboxField({
                    key: 'DepartmentManager',
                    label: this._localizationService.translate('UI.RANK.PERMISSION.DEPARTMENT_MANAGER.TITLE'),
                    helpLabel: this._localizationService.translate('UI.RANK.PERMISSION.DEPARTMENT_MANAGER.HELP_LABEL'),
                    value: updatedRank?.Permission?.DepartmentManager,
                  }));
                  fields.push(new CheckboxField({
                    key: 'Kick',
                    label: this._localizationService.translate('UI.RANK.PERMISSION.KICK.TITLE'),
                    value: updatedRank?.Permission?.Kick,
                    show: form => !form.value['DepartmentManager'],
                  }));
                  fields.push(new CheckboxField({
                    key: 'SetRank',
                    label: this._localizationService.translate('UI.RANK.PERMISSION.SET_RANK.TITLE'),
                    value: updatedRank?.Permission?.SetRank,
                    show: form => !form.value['DepartmentManager'],
                  }));
                  fields.push(new CheckboxField({
                    key: 'SetDivision',
                    label: this._localizationService.translate('UI.RANK.PERMISSION.SET_DIVISION.TITLE'),
                    value: updatedRank?.Permission?.SetDivision,
                    show: form => !form.value['DepartmentManager'],
                  }));

                  return fields;
                },
                valueResolver: new ValueResolver(rank, this._rankService.get(rank.Id)),
                buttons: {
                  submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
                },
              }).result
                .pipe(
                  filter(res => res.success),
                ).subscribe(res => options.onEditSuccess(res, rank))
            }
          },
          {
            type: 'button',
            label: this._localizationService.translate('UI.ACTION.DELETE'),
            onClick: rank => {
              this._modalConfirmService.open({
                title: 'UI.LABEL.RANK.DELETE.TITLE',
                description: this._localizationService.translate('UI.LABEL.RANK.DELETE.DESCRIPTION', {rank: `<b>${rank.Name}</b>`}),
                buttons: {
                  submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
                },
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(() => this._rankService.delete(rank.Id)),
                  this._modalService.handleResponse()
                ).subscribe(() => options.onDeleteSuccess())
            }
          },
        ]
      }));
    }

    return columns;
  }

  createButtons(options: {
    generalConfig: GeneralConfig;
    showCreateButton: boolean;
    onCreateSuccess: (response: ModalResponse) => void;
  }): Button[] {
    const buttons: Button[] = [];

    if (options.showCreateButton) {
      buttons.push({
        label: this._localizationService.translate('UI.ACTION.CREATE_RANK'),
        show: () => true,
        disabled: () => false,
        onClick: () => this._modalFormService.open({
          title: 'UI.ACTION.CREATE_RANK',
          fields: () => {
            const fields: FormInputBase<any>[] = [
              new TextField({
                key: 'Name',
                label: 'UI.LABEL.NAME.SINGULAR',
                placeholder: 'UI.LABEL.NAME.SINGULAR',
                required: true,
              }),
            ];

            if (options.generalConfig.config.Features.ProgressionSystem) {
              fields.push(new NumberField({
                key: 'RequiredXp',
                label: this._localizationService.translate('UI.LABEL.REQUIRED_XP'),
                placeholder: this._localizationService.translate('UI.LABEL.REQUIRED_XP'),
                required: true,
              }));
            }

            fields.push(new CheckboxField({
              key: 'DepartmentManager',
              label: this._localizationService.translate('UI.RANK.PERMISSION.DEPARTMENT_MANAGER.TITLE'),
              helpLabel: this._localizationService.translate('UI.RANK.PERMISSION.DEPARTMENT_MANAGER.HELP_LABEL'),
            }));
            fields.push(new CheckboxField({
              key: 'Kick',
              label: this._localizationService.translate('UI.RANK.PERMISSION.KICK.TITLE'),
              show: form => !form.value['DepartmentManager'],
            }));
            fields.push(new CheckboxField({
              key: 'SetRank',
              label: this._localizationService.translate('UI.RANK.PERMISSION.SET_RANK.TITLE'),
              show: form => !form.value['DepartmentManager'],
            }));
            fields.push(new CheckboxField({
              key: 'SetDivision',
              label: this._localizationService.translate('UI.RANK.PERMISSION.SET_DIVISION.TITLE'),
              show: form => !form.value['DepartmentManager'],
            }));

            return fields;
          },
          buttons: {
            submit: {
              variant: 'primary',
              label: 'UI.ACTION.CREATE',
            },
          },
        }).result
          .pipe(
            filter(res => res.success),
          ).subscribe(res => options.onCreateSuccess(res))
      });
    }

    return buttons;
  }

  mapRanksToFormattedRanks(page: Page<Rank>): Page<FormattedRank> {
    page.Items = page.Items.map(rank => {
      return {
        ...rank,
        Permissions: this.rankPermissionResolver(rank)
      };
    });

    return page as Page<FormattedRank>;
  }

  private rankPermissionResolver(rank: Rank): string {
    const permissions: string[] = [];

    if (rank.Permission.Kick) {
      permissions.push(this._localizationService.translate('UI.RANK.PERMISSION.KICK.TITLE'));
    }
    if (rank.Permission.SetRank) {
      permissions.push(this._localizationService.translate('UI.RANK.PERMISSION.SET_RANK.TITLE'));
    }
    if (rank.Permission.SetDivision) {
      permissions.push(this._localizationService.translate('UI.RANK.PERMISSION.SET_DIVISION.TITLE'));
    }
    if (rank.Permission.DepartmentManager) {
      permissions.push(this._localizationService.translate('UI.RANK.PERMISSION.DEPARTMENT_MANAGER.TITLE'));
    }

    return permissions.length > 0 ? permissions.join(', ') : '-';
  }

}
