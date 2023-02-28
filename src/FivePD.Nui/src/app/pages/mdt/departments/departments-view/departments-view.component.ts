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
  User, ValueResolver
} from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '@services/localization/localization.service';
import { UserState } from '@store/user.state';
import { filter, Observable, Subject, switchMap, takeUntil } from 'rxjs';
import { Select } from '@ngxs/store';
import { ModalService } from '@services/modal/modal.service';
import { UserService } from '@services/user/user.service';
import { ModalFormComponent } from '../../../../components/modal/modal-form/modal-form.component';
import { createDepartmentCallsignFormatInputField } from '../../../../helpers/forms/department';
import { CustomValidators } from '../../../../helpers/validators';

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
      label: this._localization.translate('Nui.Actions.CreateRank'),
      show: () => this.user?.Admin ?? false,
      disabled: () => false,
      onClick: () => this._modal.open(ModalFormComponent, {
        title: 'Nui.Actions.CreateRank',
        fields: () => [
          new TextInput({
            key: 'Name',
            label: 'Nui.Labels.Name',
            placeholder: 'Nui.Labels.Name',
            required: true,
          }),
          new CheckboxGroup({
            key: 'Permission',
            label: 'Nui.Labels.Permissions',
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
            label: 'Nui.Actions.Cancel',
          },
          submit: {
            variant: 'primary',
            label: 'Nui.Actions.Create',
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
          this._modal.handleResponse()
        ).subscribe(() => this.getRanks(true))
    }
  ];
  ranksCollation: Collation = {
    Page: 1,
  };
  ranksColumns: Column<FormattedRank>[] = [
    new Column<FormattedRank>({
      header: this._localization.translate('Nui.Labels.Name'),
      accessorKey: 'Name',
      filter: {
        label: this._localization.translate('Nui.Labels.SearchByName'),
      },
    }),
    new Column<FormattedRank>({
      header: this._localization.translate('Nui.Labels.Permissions'),
      accessorKey: 'Permissions',
    }),
    new Column<FormattedRank>({
      header: this._localization.translate('Nui.Labels.Actions'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Delete'),
          onClick: rank => {
            this._modal.open(ModalFormComponent, {
              title: 'Nui.Labels.DeleteRankTitle',
              description: this._localization.translate('Nui.Labels.DeleteRankDescription', {rank: `<b>${rank.Name}</b>`}),
              buttons: {
                cancel: {variant: 'secondary', label: 'Nui.Actions.Cancel'},
                submit: {variant: 'danger', label: 'Nui.Actions.Delete'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._departmentService.deleteRank(rank.Id)),
                this._modal.handleResponse()
              ).subscribe(() => this.getRanks(true))
          }
        },
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Edit'),
          onClick: rank => {
            this._modal.open<FormattedRank>(ModalFormComponent, {
              title: 'Nui.Labels.EditRankTitle',
              fields: updatedRank => [
                new TextInput({
                  key: 'Name',
                  label: this._localization.translate('Nui.Labels.Name'),
                  placeholder: this._localization.translate('Nui.Labels.Name'),
                  required: true,
                  value: updatedRank.Name,
                }),
                new CheckboxGroup({
                  key: 'Permission',
                  label: 'Nui.Labels.Permissions',
                  checkboxGroup: [
                    new Checkbox({key: 'Kick', label: 'Kick label', value: updatedRank.Permission.Kick}),
                    new Checkbox({key: 'SetRank', label: 'Set rank label', value: updatedRank.Permission.SetRank}),
                    new Checkbox({
                      key: 'SetCallsign',
                      label: 'Set callsign label',
                      value: updatedRank.Permission.SetCallsign
                    }),
                  ]
                }),
              ],
              valueResolver: new ValueResolver(rank, this._departmentService.getRank(rank.Id)),
              buttons: {
                cancel: {variant: 'secondary', label: 'Nui.Actions.Cancel'},
                submit: {variant: 'primary', label: 'Nui.Actions.Save'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(res => this._departmentService.editRank({
                  DepartmentId: this.department?.Id,
                  RankId: rank.Id,
                  Name: res.value!['Name'],
                  Permission: res.value!['Permission']
                })),
                this._modal.handleResponse()
              ).subscribe(() => this.getRanks(true))
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
      header: this._localization.translate('Nui.Labels.Name'),
      accessorKey: 'Name',
      filter: {
        label: this._localization.translate('Nui.Labels.SearchByName'),
      },
    }),
    new Column<FormattedUser>({
      header: this._localization.translate('Nui.Labels.Rank'),
      accessorKey: 'RankName',
      show: () => !this.user?.Admin ?? false,
    }),
    new Column<FormattedUser>({
      header: this._localization.translate('Nui.Labels.Division'),
      accessorKey: 'DivisionName',
      show: () => !this.user?.Admin ?? false,
    }),
    new Column<FormattedUser>({
      header: this._localization.translate('Nui.Labels.Rank'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'select',
          label: this._localization.translate('Nui.Labels.NoRank'),
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
      header: this._localization.translate('Nui.Labels.Division'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'select',
          label: this._localization.translate('Nui.Labels.NoDivision'),
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
      label: this._localization.translate('Nui.Actions.CreateDivision'),
      show: () => this.user?.Admin ?? false,
      disabled: () => false,
      onClick: () => this._modal.open(ModalFormComponent, {
        title: 'Nui.Actions.CreateDivision',
        fields: () => [
          new TextInput({
            key: 'Name',
            label: 'Nui.Labels.Name',
            placeholder: 'Nui.Labels.Name',
            required: true,
          })
        ],
        buttons: {
          cancel: {variant: 'secondary', label: 'Nui.Actions.Cancel'},
          submit: {variant: 'primary', label: 'Nui.Actions.Create'},
        },
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => this._departmentService.createDivision({
            DepartmentId: this.department?.Id,
            Name: res.value['Name']
          })),
          this._modal.handleResponse()
        ).subscribe(() => this.getDivisions(true))
    }
  ];
  divisionsColumns: Column<Division>[] = [
    new Column<Division>({
      header: this._localization.translate('Nui.Labels.Name'),
      accessorKey: 'Name',
      filter: {
        label: this._localization.translate('Nui.Labels.SearchByName'),
      },
    }),
    new Column<Division>({
      header: this._localization.translate('Nui.Labels.Actions'),
      show: () => this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Delete'),
          onClick: division => this._modal.open(ModalFormComponent, {
            title: 'Nui.Labels.DeleteDivisionTitle',
            description: this._localization.translate('Nui.Labels.DeleteDivisionDescription', {division: `<b>${division.Name}</b>`}),
            buttons: {
              cancel: {variant: 'secondary', label: 'Nui.Actions.Cancel'},
              submit: {variant: 'danger', label: 'Nui.Actions.Delete'},
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._departmentService.deleteDivision(division.Id)),
              this._modal.handleResponse()
            ).subscribe(() => this.getDivisions(true))
        },
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Edit'),
          onClick: division => {
            this._modal.open<Division>(ModalFormComponent, {
              title: 'Nui.Labels.EditDivision',
              fields: updatedDivision => [
                new TextInput({
                  key: 'Name',
                  label: 'Nui.Labels.Name',
                  placeholder: 'Nui.Labels.Name',
                  required: true,
                  value: updatedDivision.Name,
                })
              ],
              valueResolver: new ValueResolver(division, this._departmentService.getDivision(division.Id)),
              buttons: {
                cancel: {variant: 'secondary', label: 'Nui.Actions.Cancel'},
                submit: {variant: 'primary', label: 'Nui.Actions.Save'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(res => this._departmentService.editDivision({
                  DivisionId: division.Id,
                  Name: res.value['Name'],
                })),
                this._modal.handleResponse()
              ).subscribe(() => this.getDivisions(true))
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
  ) {
  }

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
    return `${this._localization.translate('Nui.Labels.Department')} ${(this.department?.Name ? '(' + this.department?.Name + ')' : '')}`;
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
      permissions += `${this._localization.translate('Nui.Actions.Kick')}, `;
    }
    if (rank.Permission.SetRank) {
      permissions += `${this._localization.translate('Nui.Actions.SetRank')}, `;
    }
    if (rank.Permission.SetCallsign) {
      permissions += `${this._localization.translate('Nui.Actions.SetCallsign')}, `;
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
        label: 'Nui.Labels.Name',
        // TODO fix: asyncValidators: [CustomValidators.nameShouldNotBeTaken((value) => this._departmentService.checkIfDepartmentNameTaken(value, this.department!.Id, true))],
        value: this.department.Name,
      }),
      new NumberInput({
        key: 'MaxUsers',
        label: 'Nui.Labels.MaxNumberOfMembers',
        required: true,
        value: this.department.MaxUsers,
        helpLabel: 'Nui.Labels.MaxNumberOfMembersHelpLabel',
      }),
      new Dropdown({
        key: 'DefaultRankId',
        label: 'Nui.Labels.DefaultRank',
        required: false,
        value: this.department.DefaultRankId ?? -1,
        helpLabel: 'Nui.Labels.DefaultRankDescription',
        dropdownOptions: {
          label: this._localization.translate('Nui.Labels.NoDefaultRank'),
          selectedItemResolver: (item) => item === this.department!.DefaultRankId,
          items: this.ranksDropdown,
          size: 'normal',
        },
      }),
      new TextInput({
        key: 'Callsign',
        label: 'Nui.Labels.Callsign',
        helpLabel: 'Nui.Labels.CallsignHelpLabel',
        validators: [CustomValidators.callsign()],
        value: this.department.Callsign,
      }),
      createDepartmentCallsignFormatInputField(this._localization, this.department),
    ];
  }
}
