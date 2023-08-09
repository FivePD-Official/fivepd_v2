import { Component, OnInit } from '@angular/core';
import {
  BaseNuiEvent,
  Button,
  Checkbox,
  CheckboxGroup,
  Collation,
  Column,
  Department,
  Division,
  Dropdown,
  DropdownItem,
  FormInputBase,
  FormSubmitEvent, GeneralConfig,
  NumberInput,
  Page,
  Rank,
  TextInput,
  User
} from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '@services/localization/localization.service';
import { filter, switchMap } from 'rxjs';
import { GlobalState } from '@store';
import { createDepartmentCallsignFormatInputField } from '../../../../helpers/forms/department';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { UserRole, UserService } from '@services/user/user.service';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { CustomValidators } from '../../../../helpers/validators';
import { ModalService } from '@services/modals/modal/modal.service';
import { Store } from '@ngrx/store';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NuiEventType } from "@enums";
import { EventService } from "@services/event/event.service";
import { ConfigService } from "@services/config/config.service";

interface FormattedRank extends Rank {
  Permissions: string;
}

interface FormattedUser extends User {
  RankName: string;
  DivisionName: string;
}

@Component({
  selector: 'app-departments-view',
  templateUrl: './departments-view.component.html',
  styleUrls: ['./departments-view.component.sass']
})
export class DepartmentsViewComponent implements OnInit {

  user?: User;
  department?: Department;
  isUserInDepartment: boolean = false;
  generalConfig?: GeneralConfig;

  ranks?: Page<FormattedRank>;
  ranksButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_RANK'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'UI.ACTION.CREATE_RANK',
        fields: () => {
          const fields: FormInputBase<any>[] = [
            new TextInput({
              key: 'Name',
              label: 'UI.LABEL.NAME.SINGULAR',
              placeholder: 'UI.LABEL.NAME.SINGULAR',
              required: true,
            }),
          ];

          if (this.generalConfig?.config.FeatureFlags.ProgressionSystem) {
            fields.push(new NumberInput({
              key: 'RequiredXp',
              label: this._localization.translate('UI.LABEL.REQUIRED_XP'),
              placeholder: this._localization.translate('UI.LABEL.REQUIRED_XP'),
              required: true,
            }))
          }

          fields.push(new CheckboxGroup({
            key: 'Permission',
            label: 'UI.LABEL.PERMISSIONS',
            checkboxGroup: [
              new Checkbox({
                key: 'Kick',
                label: 'Kick label',
              }),
              new Checkbox({
                key: 'SetRank',
                label: 'Set rank label',
              }),
              new Checkbox({
                key: 'SetCallsign',
                label: 'Set callsign label',
              }),
            ]
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
          switchMap(res => this._departmentService.createRank({
            DepartmentId: this.department!.Id,
            Name: res.value['Name'],
            Permission: res.value['Permission']
          })),
          this._modalService.handleResponse()
        ).subscribe(() => this.getRanks(true))
    }
  ];
  ranksCollation: Collation = {
    Page: 1,
  };
  ranksColumns: Column<FormattedRank>[] = [];

  users?: Page<FormattedUser>;
  usersCollation: Collation = {
    Page: 1,
  };
  usersColumns: Column<FormattedUser>[] = [];

  divisions?: Page<Division>;
  divisionsCollation: Collation = {
    Page: 1,
  };
  divisionsButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_DIVISION'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      disabled: () => false,
      onClick: () => {
        this._modalFormService.open({
          title: 'UI.ACTION.CREATE_DIVISION',
          fields: () => [
            new TextInput({
              key: 'Name',
              label: 'UI.LABEL.NAME.SINGULAR',
              placeholder: 'UI.LABEL.NAME.SINGULAR',
              required: true,
            })
          ],
          buttons: {
            submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
          },
        }).result
          .pipe(
            filter(res => res.success),
            switchMap(res => this._departmentService.createDivision({
              DepartmentId: this.department?.Id,
              Name: res.value['Name']
            })),
            this._modalService.handleResponse()
          ).subscribe(() => this.getDivisions(true))
      }
    }
  ];
  divisionsColumns: Column<Division>[] = [
    new Column<Division>({
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      accessorKey: 'Name',
      filter: {
        label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
      },
    }),
    new Column<Division>({
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          onClick: division => {
            this._modalFormService.open<Division>({
              title: 'UI.LABEL.DIVISION.EDIT',
              fields: updatedDivision => [
                new TextInput({
                  key: 'Name',
                  label: 'UI.LABEL.NAME.SINGULAR',
                  placeholder: 'UI.LABEL.NAME.SINGULAR',
                  required: true,
                  value: updatedDivision.Name,
                })
              ],
              valueResolver: new ValueResolver(division, this._departmentService.getDivision(division.Id)),
              buttons: {
                submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(res => this._departmentService.editDivision({
                  DivisionId: division.Id,
                  Name: res.value['Name'],
                })),
                this._modalService.handleResponse()
              ).subscribe(() => this.getDivisions(true))
          }
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          onClick: division => {
            this._modalConfirmService.open({
              title: 'UI.LABEL.DIVISION.DELETE.TITLE',
              description: this._localization.translate('UI.LABEL.DIVISION.DELETE.DESCRIPTION', {division: `<b>${division.Name}</b>`}),
              buttons: {
                submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._departmentService.deleteDivision(division.Id)),
                this._modalService.handleResponse()
              ).subscribe(() => this.getDivisions(true))
          }
        },
      ]
    })
  ];

  departmentSettingsForm: FormInputBase<any>[] = [];

  constructor(
    public readonly userService: UserService,
    private readonly _router: Router,
    private readonly _route: ActivatedRoute,
    private readonly _localization: LocalizationService,
    private readonly _departmentService: DepartmentService,
    private readonly _modalService: ModalService,
    private readonly _store: Store<GlobalState>,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _eventService: EventService,
    private readonly _configService: ConfigService,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.user = state.user.details.User;
      this.setIsUserInDepartment();
    });

    _route.params.pipe(takeUntilDestroyed()).subscribe(() => {
      this.getDepartment();
    });

    _eventService.onEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(() => {
      this.getDepartment();
    });
  }

  ngOnInit(): void {
    this.getGeneralConfig();
  }

  private getGeneralConfig(): void {
    this._configService.getGeneralConfig().subscribe(res => {
      this.generalConfig = res;
      this.ranksColumns = this.createRanksColumns();
      this.usersColumns = this.createUsersColumns();
    });
  }

  private createRanksColumns(): Column<FormattedRank>[] {
    const columns = [
      new Column<FormattedRank>({
        header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
        accessorKey: 'Name',
        filter: {
          label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
        },
      }),
      new Column<FormattedRank>({
        header: this._localization.translate('UI.LABEL.PERMISSIONS'),
        accessorKey: 'Permissions',
      }),
    ];

    if (this.generalConfig?.config.FeatureFlags.ProgressionSystem) {
      columns.push(new Column<FormattedRank>({
        header: this._localization.translate('UI.LABEL.REQUIRED_XP'),
        accessorKey: 'RequiredXp',
      }));
    }

    columns.push(new Column<FormattedRank>({
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          onClick: rank => {
            this._modalFormService.open<FormattedRank>({
              title: 'UI.LABEL.RANK.EDIT',
              fields: updatedRank => {
                const fields: FormInputBase<any>[] = [
                  new TextInput({
                    key: 'Name',
                    label: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                    placeholder: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                    required: true,
                    value: updatedRank.Name,
                  }),
                ];

                if (this.generalConfig?.config.FeatureFlags.ProgressionSystem) {
                  fields.push(new NumberInput({
                    key: 'RequiredXp',
                    label: this._localization.translate('UI.LABEL.REQUIRED_XP'),
                    placeholder: this._localization.translate('UI.LABEL.REQUIRED_XP'),
                    required: true,
                    value: updatedRank.RequiredXp,
                  }))
                }

                fields.push(new CheckboxGroup({
                  key: 'Permission',
                  label: 'UI.LABEL.PERMISSIONS',
                  checkboxGroup: [
                    new Checkbox({key: 'Kick', label: 'Kick label', value: updatedRank.Permission.Kick}),
                    new Checkbox({key: 'SetRank', label: 'Set rank label', value: updatedRank.Permission.SetRank}),
                    new Checkbox({
                      key: 'SetCallsign',
                      label: 'Set callsign label',
                      value: updatedRank.Permission.SetCallsign
                    }),
                  ]
                }));

                return fields;
              },
              valueResolver: new ValueResolver(rank, this._departmentService.getRank(rank.Id)),
              buttons: {
                submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(res => this._departmentService.editRank({
                  DepartmentId: this.department?.Id,
                  RankId: rank.Id,
                  Name: res.value!['Name'],
                  Permission: res.value!['Permission'],
                  RequiredXp: res.value!['RequiredXp'],
                })),
                this._modalService.handleResponse()
              ).subscribe(() => this.getRanks(true))
          }
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          onClick: rank => {
            this._modalConfirmService.open({
              title: 'UI.LABEL.RANK.DELETE.TITLE',
              description: this._localization.translate('UI.LABEL.RANK.DELETE.DESCRIPTION', {rank: `<b>${rank.Name}</b>`}),
              buttons: {
                submit: {variant: 'danger', label: 'UI.ACTION.DELETE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._departmentService.deleteRank(rank.Id)),
                this._modalService.handleResponse()
              ).subscribe(() => this.getRanks(true))
          }
        },
      ]
    }))

    return columns;
  }

  private createUsersColumns(): Column<FormattedUser>[] {
    const columns = [
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
        accessorKey: 'Name',
        filter: {
          label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
        },
      }),
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.SINGULAR'),
        accessorKey: 'RankName',
        show: () => !this.userService.userHasRole(UserRole.Admin),
      }),
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
        accessorKey: 'DivisionName',
        show: () => !this.userService.userHasRole(UserRole.Admin),
      }),
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.SINGULAR'),
        show: () => this.userService.userHasRole(UserRole.Admin),
        actions: [
          {
            type: 'select',
            label: this._localization.translate('UI.LABEL.NO_RANK'),
            selectedItemResolver: (user, dropdownItemKey) => user.RankId === dropdownItemKey,
            items: () => this.ranksDropdown,
            onSelect: (user, selectedId) => {
              this.userService.changeRank({
                UserId: user.Id,
                IdToChange: selectedId,
              }).subscribe();
            }
          }
        ]
      }),
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
        show: () => this.userService.userHasRole(UserRole.Admin),
        actions: [
          {
            type: 'select',
            label: this._localization.translate('UI.LABEL.NO_DIVISION'),
            selectedItemResolver: (user, dropdownItemKey) => user.DivisionId === dropdownItemKey,
            items: () => this.divisionsDropdown,
            onSelect: (user, selectedId) => {
              this.userService.changeDivision({
                UserId: user.Id,
                IdToChange: selectedId,
              }).subscribe();
            }
          },
        ]
      }),
    ];

    if (this.generalConfig?.config.FeatureFlags.ProgressionSystem) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.FIXED'),
        show: () => this.userService.userHasRole(UserRole.Admin),
        actions: [
          {
            type: 'checkbox',
            label: '',
            checkedResolver: user => user.FixedRank,
            onCheck: (user, checked) => {
              this.userService.changeFixedRank({
                UserId: user.Id,
                FixedRank: checked,
              }).subscribe();
            },
          }
        ]
      }));
    }

    return columns;
  }

  setIsUserInDepartment(): void {
    this.isUserInDepartment = this.user?.DepartmentId === this.department?.Id;
  }

  get title(): string {
    return `${this._localization.translate('UI.LABEL.DEPARTMENT.SINGULAR')} ${(this.department?.Name ? '(' + this.department?.Name + ')' : '')}`;
  }

  get ranksDropdown(): DropdownItem[] {
    return this.department?.Ranks?.map(rank => {
      return {
        key: rank.Id,
        label: rank.Name,
      };
    }) ?? [];
  }

  get divisionsDropdown(): DropdownItem[] {
    return this.department?.Divisions?.map(division => {
      return {
        key: division.Id,
        label: division.Name,
      };
    }) ?? [];
  }

  getDepartment(): void {
    this._departmentService.getDepartment(this._route.snapshot.params['id']).subscribe(res => {
      this.department = res;
      this.getRanks();
      this.getDivisions();
      this.getUsers();
      this.setIsUserInDepartment();
      this.createDepartmentSettingsForm();
    });
  }

  getRanks(remapUsers: boolean = false): void {
    if (!this.department) {
      return;
    }

    this._departmentService.getRanks(this.department.Id, this.ranksCollation).subscribe(res => {
      res.Items = res.Items.map(rank => {
        return {
          ...rank,
          Permissions: this.rankPermissionResolver(rank)
        };
      });
      this.ranks = res as Page<FormattedRank>;

      if (remapUsers) {
        this.getDepartment();
      }
    });
  }

  getDivisions(remapUsers: boolean = false): void {
    if (!this.department) {
      return;
    }

    this._departmentService.getDivisions(this.department.Id, this.divisionsCollation).subscribe(res => {
      this.divisions = res;

      if (remapUsers) {
        this.getDepartment();
      }
    });
  }

  getUsers(): void {
    if (!this.department) {
      return;
    }

    this._departmentService.getDepartmentUsers(this.department.Id, this.usersCollation).subscribe(res => {
      res.Items = res.Items.map(user => {
        return {
          ...user,
          RankName: this.department?.Ranks?.find(rank => rank.Id === user.RankId)?.Name ?? '-',
          DivisionName: this.department?.Divisions?.find(division => division.Id === user.DivisionId)?.Name ?? '-',
        };
      });
      this.users = res as Page<FormattedUser>;
    });
  }

  onRanksCollationChange(collation: Collation): void {
    this.ranksCollation = collation;
    this.getRanks();
  }

  onDivisionsCollationChange(collation: Collation): void {
    this.divisionsCollation = collation;
    this.getDivisions();
  }

  onUsersCollationChange(collation: Collation): void {
    this.usersCollation = collation;
    this.getUsers();
  }

  joinDepartment(): void {
    this._departmentService.joinDepartment(this.department!.Id).subscribe(res => {
      if (res.Status) {
        this.userService.refreshData();
        this.getUsers();
      }
    });
  }

  leaveDepartment(): void {
    this._departmentService.leaveDepartment(this.department!.Id).subscribe(res => {
      if (res.Status) {
        this.userService.refreshData();
        this._router.navigate(['mdt', 'dashboard']);
      }
    });
  }

  onDepartmentSettingsSubmit(event: FormSubmitEvent): void {
    this._departmentService.editDepartment({
      DepartmentId: this.department!.Id,
      ...event.value
    }).subscribe(res => {
      if (res.Status) {
        this.getDepartment();
        this.userService.refreshData();
        event.markAsPristine();
      }
    });
  }

  private rankPermissionResolver(rank: Rank): string {
    let permissions = '';

    if (rank.Permission.Kick) {
      permissions += `${this._localization.translate('UI.ACTION.KICK')}, `;
    }
    if (rank.Permission.SetRank) {
      permissions += `${this._localization.translate('UI.ACTION.SET_RANK')}, `;
    }
    if (rank.Permission.SetCallsign) {
      permissions += `${this._localization.translate('UI.ACTION.SET_CALLSIGN')}, `;
    }

    return permissions.length ? permissions.slice(0, -2) : '-';
  }

  private createDepartmentSettingsForm(): void {
    if (!this.department) {
      return;
    }

    this.departmentSettingsForm = [
      new TextInput({
        key: 'Name',
        required: true,
        label: 'UI.LABEL.NAME.SINGULAR',
        asyncValidators: [CustomValidators.nameShouldNotBeTaken(value => this._departmentService.checkIfDepartmentNameTaken(value, this.department!.Id, true))],
        value: this.department.Name,
      }),
      new NumberInput({
        key: 'MaxUsers',
        label: 'UI.LABEL.MAX_MEMBERS.TITLE',
        required: true,
        value: this.department.MaxUsers,
        helpLabel: 'UI.LABEL.MAX_MEMBERS.HELP_LABEL',
      }),
      new Dropdown({
        key: 'DefaultRankId',
        label: 'UI.LABEL.DEFAULT_RANK.TITLE',
        required: false,
        value: this.department.DefaultRankId ?? -1,
        helpLabel: 'UI.LABEL.DEFAULT_RANK.DESCRIPTION',
        dropdownOptions: {
          label: this._localization.translate('UI.LABEL.NO_DEFAULT_RANK'),
          selectedItemResolver: (item) => item === this.department!.DefaultRankId,
          items: this.ranksDropdown,
          size: 'normal',
        },
      }),
      new TextInput({
        key: 'Callsign',
        label: 'UI.LABEL.CALLSIGN.TITLE',
        helpLabel: 'UI.LABEL.CALLSIGN.HELP_LABEL',
        validators: [CustomValidators.onlyLetters()],
        value: this.department.Callsign,
      }),
      createDepartmentCallsignFormatInputField(this._localization, this.department),
    ];
  }

  protected readonly UserRole = UserRole;
}
