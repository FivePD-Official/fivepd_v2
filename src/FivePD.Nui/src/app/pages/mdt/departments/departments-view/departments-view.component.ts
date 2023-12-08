import {Component, OnInit} from '@angular/core';
import {
  ActionResponse,
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
  FormSubmitEvent,
  GeneralConfig,
  IconType,
  NumberInput,
  Page,
  PlayerDetailsDTO,
  Rank,
  TextInput,
  User
} from '@models';
import {DepartmentService} from '@services/department/department.service';
import {ActivatedRoute, Router} from '@angular/router';
import {LocalizationService} from '@services/localization/localization.service';
import {filter, Observable, switchMap} from 'rxjs';
import {GlobalState} from '@store';
import {createDepartmentCallsignFormatInputField} from '../../../../helpers/forms/department';
import {ModalConfirmService} from '@services/modals/modal-confirm/modal-confirm.service';
import {UserRole, UserService} from '@services/user/user.service';
import {ValueResolver} from '@services/modals/modal-form/modal-form-value-resolver';
import {ModalFormService} from '@services/modals/modal-form/modal-form.service';
import {CustomValidators} from '../../../../helpers/validators';
import {ModalService} from '@services/modals/modal/modal.service';
import {Store} from '@ngrx/store';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {EventService} from "@services/event/event.service";
import {ConfigService} from "@services/config/config.service";
import {TranslatePipe} from '../../../../pipes/translate.pipe';
import {GridComponent} from '../../../../components/grid/grid.component';
import {DynamicFormComponent} from '../../../../components/dynamic-form/dynamic-form.component';
import {ContainerComponent} from '../../../../components/container/container.component';
import {NgIf} from '@angular/common';
import {TippyModule} from '@ngneat/helipopper';
import {ButtonComponent} from '../../../../components/button/button.component';
import {PageHeaderComponent} from '../../../../components/page-header/page-header.component';

interface FormattedRank extends Rank {
  Permissions: string;
}

interface FormattedUser extends User {
  Callsign: string;
  RankName: string;
  DivisionName: string;
}

type Mode = 'view' | 'edit';

@Component({
  selector: 'app-departments-view',
  templateUrl: './departments-view.component.html',
  styleUrls: ['./departments-view.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ButtonComponent, TippyModule, NgIf, ContainerComponent, DynamicFormComponent, GridComponent, TranslatePipe]
})
export class DepartmentsViewComponent implements OnInit {

  user?: PlayerDetailsDTO;
  department?: Department;
  isUserInDepartment: boolean = false;
  generalConfig?: GeneralConfig;

  ranks?: Page<FormattedRank>;
  ranksButtons: Button[] = [];
  ranksCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  ranksColumns: Column<FormattedRank>[] = [];

  users?: Page<FormattedUser>;
  usersCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  usersColumns: Column<FormattedUser>[] = [];

  divisions?: Page<Division>;
  divisionsCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  divisionsButtons: Button[] = [];
  divisionsColumns: Column<Division>[] = [];

  departmentSettingsForm: FormInputBase<any>[] = [];
  mode: Mode = 'view';

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
      this.user = state.user.details;
      this.setIsUserInDepartment();
    });

    _route.data.pipe(takeUntilDestroyed()).subscribe(data => {

      this.mode = data['mode'];
    });

    _route.params.pipe(takeUntilDestroyed()).subscribe(params => {
      this.getDepartment();
    });
  }

  ngOnInit(): void {
    this.getGeneralConfig();
    this.getDepartment();
  }

  private getGeneralConfig(): void {
    this._configService.getGeneralConfig().subscribe(res => {
      this.generalConfig = res;
      this.ranksColumns = this.createRanksColumns();
      this.ranksButtons = this.createRanksButtons();
      this.divisionsColumns = this.createDivisionsColumns();
      this.divisionsButtons = this.createDivisionsButtons();
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

    if (this.generalConfig?.config.Features.ProgressionSystem) {
      columns.push(new Column<FormattedRank>({
        header: this._localization.translate('UI.LABEL.REQUIRED_XP'),
        accessorKey: 'RequiredXp',
      }));
    }

    if (this.mode === 'edit' && this.userService.userIsDeptAdmin()) {
      columns.push(new Column<FormattedRank>({
        header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
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

                  if (this.generalConfig?.config.Features.ProgressionSystem) {
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
                      new Checkbox({key: 'Kick', label: 'Kick member', value: updatedRank.Permission.Kick}),
                      new Checkbox({key: 'SetRank', label: 'Set rank', value: updatedRank.Permission.SetRank}),
                      new Checkbox({
                        key: 'SetDivision',
                        label: 'Set division',
                        value: updatedRank.Permission.SetDivision
                      }), new Checkbox({
                        key: 'DeptAdmin',
                        label: 'Dept. Admin',
                        value: updatedRank.Permission.DeptAdmin
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
      }));
    }

    return columns;
  }

  private createRanksButtons(): Button[] {
    const buttons: Button[] = [];

    if (this.mode === 'edit' && this.userService.userIsDeptAdmin()) {
      buttons.push({
        label: this._localization.translate('UI.ACTION.CREATE_RANK'),
        show: () => this.userService.userIsDeptAdmin(),
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

            if (this.generalConfig?.config.Features.ProgressionSystem) {
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
                new Checkbox({
                  key: 'DeptAdmin',
                  label: 'Dept. Admin',
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
      });
    }

    return buttons;
  }

  private createDivisionsColumns(): Column<Division>[] {
    const columns: Column<Division>[] = [
      new Column<Division>({
        header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
        accessorKey: 'Name',
        filter: {
          label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
        },
      }),
    ];

    if (this.mode === 'edit' && this.userService.userIsDeptAdmin()) {
      columns.push(new Column<Division>({
        header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
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
      }));
    }

    return columns;
  }

  private createDivisionsButtons(): Button[] {
    const buttons: Button[] = [];

    if (this.mode === 'edit' && this.userService.userIsDeptAdmin()) {
      buttons.push({
        label: this._localization.translate('UI.ACTION.CREATE_DIVISION'),
        show: () => true,
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
      });
    }

    return buttons;
  }

  private createUsersColumns(): Column<FormattedUser>[] {
    const columns = [
      new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
        accessorKey: 'Name',
        filter: {
          label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
        },
      })
    ];

    if (this.userService.userIsDeptAdmin()) {

      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.CALLSIGN.TITLE'),
        accessorKey: 'Callsign',
        cellAction: {
          icon: IconType.Pencil,
          onClick: item => {
            let updatedCallsign = '';
            this._modalFormService.open<FormattedUser>({
                title: "",
                fields: updatedUser => {
                  const fields: FormInputBase<any>[] = [
                    new TextInput({
                      key: 'Callsign',
                      label: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                      placeholder: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                      required: true,
                      value: item.Callsign,
                    }),
                  ];
                  return fields;
                },
                buttons: {
                  submit: {variant: 'primary', label: 'UI.ACTION.SAVE'},
                },
              }
            ).result.pipe(
              filter((res) => res.success),
              switchMap((res) => this.userService.updateUserDetails({Id: item.Id, Callsign: res.value['Callsign']})),
              this._modalService.handleResponse(),
            ).subscribe(res => {
              if (res.Status) {
                item.Callsign = updatedCallsign;
              }
              this.getDivisions(true);
            })
          }
        }
      }))
    } else {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.CALLSIGN.TITLE'),
        accessorKey: 'Callsign',
      }))
    }


    if (this.userService.userIsDeptAdmin() || this.user?.User?.Rank?.Permission.SetRank) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.SINGULAR'),
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
      }));
    } else {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.SINGULAR'),
        accessorKey: 'RankName',
      }));
    }
    if (this.userService.userIsDeptAdmin() || this.user?.User.Rank?.Permission.SetDivision) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
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
      }));
    } else {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
        accessorKey: 'DivisionName',
      }));
    }

    if (this.generalConfig?.config.Features.ProgressionSystem) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.FIXED'),
        show: () => this.userService.userIsDeptAdmin(),
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

    if (this.userService.userIsDeptAdmin() || this.user?.User?.Rank?.Permission.Kick) {
      columns.push(new Column<FormattedUser>({
        header: 'Actions',
        actions: [
          {
            type: 'button',
            label: this._localization.translate('UI.ACTION.KICK.TITLE'),
            disabled: user => user ? user.Id === this.user?.User.Id : false,
            onClick: user => {
              this._modalConfirmService.open({
                title: 'UI.ACTION.KICK.TITLE',
                description: this._localization.translate('UI.LABEL.DEPARTMENT.KICK_CONFIRMATION'),
                buttons: {
                  submit: {variant: 'danger', label: 'UI.ACTION.KICK.TITLE'},
                },
              }).result
                .pipe(
                  filter(res => res.success),
                  switchMap(() => this._departmentService.kickPlayerFromUI(user.Id)),
                  this._modalService.handleResponse()
                ).subscribe(v => this.getUsers())
            }
          },
        ]
      }))
    }

    return columns;
  }

  setIsUserInDepartment(): void {
    this.isUserInDepartment = this.user?.User?.DepartmentId === this.department?.Id;
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
    const Id = this._route.snapshot.params['id'];
    if (Id === null || Id === undefined) return;
    this._departmentService.getDepartment(Id).subscribe(res => {
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
          Callsign: user.Callsign,
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
    const permissions: string[] = [];

    if (rank.Permission.Kick) {
      permissions.push(this._localization.translate('UI.ACTION.KICK.TITLE'));
    }
    if (rank.Permission.SetRank) {
      permissions.push(this._localization.translate('UI.ACTION.SET_RANK'));
    }
    if (rank.Permission.SetDivision) {
      permissions.push(this._localization.translate('UI.ACTION.SET_DIVISION'));
    }
    if (rank.Permission.DeptAdmin) {
      permissions.push(this._localization.translate('UI.ACTION.DEPT_ADMIN'))
    }

    return permissions.length > 0 ? permissions.join(', ') : '-';
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
          variant: 'secondary'
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
