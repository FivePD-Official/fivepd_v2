import { Component, OnDestroy, OnInit } from '@angular/core';
import {
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
  NumberInput,
  Page,
  Rank,
  TextInput,
  User
} from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '@services/localization/localization.service';
import { UserState } from '@store/user.state';
import { Observable, Subject, takeUntil } from 'rxjs';
import { Select } from '@ngxs/store';
import { ModalService } from '@services/modal/modal.service';
import { UserService } from '@services/user/user.service';
import { GenericNameValidator } from '@validators/generic-name-validator';
import { Validators } from '@angular/forms';
import { ModalFormComponent } from '../../../../components/modal/modal-form/modal-form.component';
import { NotificationService } from '@services/notification/notification.service';

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
export class DepartmentsViewComponent implements OnInit, OnDestroy {

  onDestroy$ = new Subject<void>();

  user?: User;
  @Select(UserState.details) user$!: Observable<User>;

  department?: Department;
  isUserInDepartment: boolean = false;

  ranks?: Page<FormattedRank>;
  ranksButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_RANK'),
      show: () => this.user?.Admin ?? false,
      disabled: () => false,
      onClick: () => this._modal.open(ModalFormComponent, {
        title: 'UI.ACTION.CREATE_RANK',
        fields: [
          new TextInput({
            key: 'Name',
            label: 'UI.LABEL.NAME.SINGULAR',
            placeholder: 'UI.LABEL.NAME.SINGULAR',
            required: true,
          }),
          new CheckboxGroup({
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
          }),
        ],
        buttons: {
          cancel: {
            variant: 'secondary',
            label: 'UI.ACTION.CANCEL',
          },
          submit: {
            variant: 'primary',
            label: 'UI.ACTION.CREATE',
          },
        },
      }).onSuccess(res => {
        this._departmentService.createRank({
          DepartmentId: this.department!.Id,
          Name: res.value!['Name'],
          Permission: res.value!['Permission']
        }).subscribe(createRes => {
          if (createRes.Status) {
            this.getRanks(true);
          }
        });
      })
    }
  ];
  ranksCollation: Collation = {
    Page: 1,
  };
  ranksColumns: Column<FormattedRank>[] = [
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
    new Column<FormattedRank>({
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          onClick: rank => {
            this._modal.open(ModalFormComponent, {
              title: 'UI.LABEL.RANK.DELETE.TITLE',
              description: this._localization.translate('UI.LABEL.RANK.DELETE.DESCRIPTION', { rank: `<b>${rank.Name}</b>`}),
              buttons: {
                cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
                submit: { variant: 'danger', label: 'UI.ACTION.DELETE' },
              },
            }).onSuccess(() => {
              this._departmentService.deleteRank(rank.Id).subscribe(deleteRes => {
                if (deleteRes.Status) {
                  this.getRanks(true);
                }
              });
            });
          }
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          onClick: rank => {
            this._modal.open(ModalFormComponent, {
              title: 'UI.LABEL.RANK.EDIT',
              fields: [
                new TextInput({
                  key: 'Name',
                  label: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                  placeholder: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
                  required: true,
                  value: rank.Name,
                }),
                new CheckboxGroup({
                  key: 'Permission',
                  label: 'UI.LABEL.PERMISSIONS',
                  checkboxGroup: [
                    new Checkbox({ key: 'Kick', label: 'Kick label', value: rank.Permission.Kick }),
                    new Checkbox({ key: 'SetRank', label: 'Set rank label', value: rank.Permission.SetRank }),
                    new Checkbox({ key: 'SetCallsign', label: 'Set callsign label', value: rank.Permission.SetCallsign }),
                  ]
                }),
              ],
              buttons: {
                cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
                submit: { variant: 'primary', label: 'UI.ACTION.SAVE' },
              },
            }).onSuccess(res => {
              this._departmentService.editRank({
                DepartmentId: this.department?.Id,
                RankId: rank.Id,
                Name: res.value!['Name'],
                Permission: res.value!['Permission']
              }).subscribe(editRes => {
                if (editRes.Status) {
                  this.getRanks(true);
                }
              });
            });
          }
        },
      ]
    })
  ];

  users?: Page<FormattedUser>;
  usersCollation: Collation = {
    Page: 1,
  };
  usersColumns: Column<FormattedUser>[] = [
    new Column<FormattedUser>({
      header:  this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      accessorKey: 'Name',
      filter: {
        label:  this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
      },
    }),
    new Column<FormattedUser>({
      header:  this._localization.translate('UI.LABEL.SINGULAR'),
      accessorKey: 'RankName',
      show: () => !this.user?.Admin ?? false,
    }),
    new Column<FormattedUser>({
      header:  this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
      accessorKey: 'DivisionName',
      show: () => !this.user?.Admin ?? false,
    }),
    new Column<FormattedUser>({
      header:  this._localization.translate('UI.LABEL.RANK.SINGULAR'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'select',
          label:  this._localization.translate('UI.LABEL.NO_RANK'),
          selectedItemResolver: (user, dropdownItemKey) => user.RankId === dropdownItemKey,
          items: () => this.ranksDropdown,
          onSelect: (user, selectedId) => {
            this._userService.changeRank({
              UserId: user.Id,
              IdToChange: selectedId,
            }).subscribe();
          }
        }
      ]
    }),
    new Column<FormattedUser>({
      header:  this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'select',
          label: this._localization.translate('UI.LABEL.NO_DIVISION'),
          selectedItemResolver: (user, dropdownItemKey) => user.DivisionId === dropdownItemKey,
          items: () => this.divisionsDropdown,
          onSelect: (user, selectedId) => {
            this._userService.changeDivision({
              UserId: user.Id,
              IdToChange: selectedId,
            }).subscribe();
          }
        },
      ]
    })
  ];

  divisions?: Page<Division>;
  divisionsCollation: Collation = {
    Page: 1,
  };
  divisionsButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_DIVISION'),
      show: () => this.user?.Admin ?? false,
      disabled: () => false,
      onClick: () => this._modal.open(ModalFormComponent, {
        title: 'UI.ACTION.CREATE_DIVISION',
        fields: [
          new TextInput({
            key: 'Name',
            label: 'UI.LABEL.NAME.SINGULAR',
            placeholder: 'UI.LABEL.NAME.SINGULAR',
            required: true,
          })
        ],
        buttons: {
          cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
          submit: { variant: 'primary', label: 'UI.ACTION.CREATE' },
        },
      }).onSuccess(res => {
        this._departmentService.createDivision({
          DepartmentId: this.department?.Id,
          Name: res.value!['Name']
        }).subscribe(createRes => {
          if (createRes.Status) {
            this.getDivisions(true);
          }
        });
      }),
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
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          onClick: division => this._modal.open(ModalFormComponent, {
            title: 'UI.LABEL.DIVISION.DELETE.TITLE',
            description: this._localization.translate('UI.LABEL.DIVISION.DELETE.DESCRIPTION', { division: `<b>${division.Name}</b>` }),
            buttons: {
              cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL'},
              submit: { variant: 'danger', label: 'UI.ACTION.DELETE'},
            },
          }).onSuccess(() => {
            this._departmentService.deleteDivision(division.Id).subscribe(deleteRes => {
              if (deleteRes.Status) {
                this.getDivisions(true);
              }
            });
          })
        },
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.EDIT'),
          onClick: division => {
            this._modal.open(ModalFormComponent, {
              title: 'UI.LABEL.DIVISION.EDIT',
              fields: [
                new TextInput({
                  key: 'Name',
                  label: 'UI.LABEL.NAME.SINGULAR',
                  placeholder: 'UI.LABEL.NAME.SINGULAR',
                  required: true,
                  value: division.Name,
                })
              ],
              buttons: {
                cancel: { variant: 'secondary', label: 'UI.ACTION.CANCEL' },
                submit: { variant: 'primary', label: 'UI.ACTION.SAVE' },
              },
            }).onSuccess(res => {
              this._departmentService.editDivision({
                DivisionId: division.Id,
                Name: res.value!['Name'],
              }).subscribe(editRes => {
                if (editRes.Status) {
                  this.getDivisions(true);
                }
              });
            })
          }
        }
      ]
    })
  ];

  departmentSettingsForm: FormInputBase<any>[] = [];

  constructor(
    private _router: Router,
    private _route: ActivatedRoute,
    private _localization: LocalizationService,
    private _departmentService: DepartmentService,
    private _userService: UserService,
    private _modal: ModalService
  ) { }

  ngOnInit(): void {
    this.user$.pipe(
      takeUntil(this.onDestroy$)
    ).subscribe(user => {
      this.user = user;
      this.setIsUserInDepartment();
    });

    this._route.params.pipe(
      takeUntil(this.onDestroy$)
    ).subscribe(() => {
      this.getDepartment();
    });
  }

  ngOnDestroy(): void {
    this.onDestroy$.next();
    this.onDestroy$.complete();
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
        this._userService.refreshData();
        this.getUsers();
      }
    });
  }

  leaveDepartment(): void {
    this._departmentService.leaveDepartment(this.department!.Id).subscribe(res => {
      if (res.Status) {
        this._userService.refreshData();
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
        this._userService.refreshData();
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

    this.departmentSettingsForm  = [
      new TextInput({
        key: 'Name',
        required: true,
        label: 'UI.LABEL.NAME.SINGULAR',
        // TODO fix: asyncValidators: [GenericNameValidator.get((value) => this._departmentService.checkIfDepartmentNameTaken(value, this.department!.Id, true))],
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
        validators: [Validators.pattern('^[a-zA-Z]+$')],
        value: this.department.Callsign,
      }),
      new TextInput({
        key: 'CallsignFormat',
        required: true,
        label: 'UI.LABEL.CALLSIGN.FORMAT.TITLE',
        helpLabel: `
            ${this._localization.translate('UI.LABEL.CALLSIGN.FORMAT.HELP_LABEL')} ${this._localization.translate('UI.LABEL.AVAILABLE_CHARACTERS')}: <br/>
            <table>
              <tr>
                <td><b>L</b><td>
                <td>(${this._localization.translate('UI.LABEL.LETTER.SINGULAR')})<td>
              </tr>
              <tr>
                <td><b>N</b><td>
                <td>(${this._localization.translate('UI.LABEL.NUMBER.SINGULAR')})<td>
              </tr>
              <tr>
                <td><b>-</b><td>
                <td>(${this._localization.translate('UI.LABEL.DASH.SINGULAR')})<td>
              </tr>
            </table>
          `,
        validators: [Validators.pattern('^[NL0-9-*]{1,10}$')],
        value: this.department.CallsignFormat
      }),
    ];
  }
}
