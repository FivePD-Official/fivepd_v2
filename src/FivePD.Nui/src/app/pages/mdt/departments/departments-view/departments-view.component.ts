import { Component, OnInit } from '@angular/core';
import {
  Button,
  CheckboxField,
  Collation,
  Column,
  Department,
  Division,
  DropdownItem,
  FormInputBase,
  FormSubmitEvent,
  GeneralConfig,
  IconType,
  NumberField,
  Page,
  PlayerDetailsDTO,
  Rank,
  SingleSelectField,
  TextField,
  User
} from '@models';
import { DepartmentService } from '@services/department/department.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '@services/localization/localization.service';
import { filter, map, switchMap } from 'rxjs';
import { GlobalState } from '@store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { UserRole, UserService } from '@services/user/user.service';
import { ValueResolver } from '@services/modals/modal-form/modal-form-value-resolver';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { CustomValidators } from '../../../../helpers/validators';
import { ModalService } from '@services/modals/modal/modal.service';
import { Store } from '@ngrx/store';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EventService } from "@services/event/event.service";
import { ConfigService } from "@services/config/config.service";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { GridComponent } from '../../../../components/grid/grid.component';
import { DynamicFormComponent } from '../../../../components/dynamic-form/dynamic-form.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import { TippyModule } from '@ngneat/helipopper';
import { ButtonComponent } from '../../../../components/button/button.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import {
  DepartmentCallsignFormatEditorComponent
} from "./_components/department-callsign-format-editor/department-callsign-format-editor.component";
import { CALLSIGN_FORMAT_ITEMS } from "./_components/department-callsign-format-editor/token";
import { CallsignEditorComponent } from "../../my-settings/_components/user-callsign-editor/callsign-editor.component";
import { CALLSIGN_FORMAT_VALUES } from "../../my-settings/_components/user-callsign-editor/token";
import { RankPageCollationFilters, RankService } from "@services/rank/rank.service";
import { FormattedRank, RankOptions } from "../../../_service/rank-options";

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
    imports: [PageHeaderComponent, ButtonComponent, TippyModule, ContainerComponent, DynamicFormComponent, GridComponent, TranslatePipe]
})
export class DepartmentsViewComponent implements OnInit {

  user?: PlayerDetailsDTO;
  department?: Department;
  isUserInDepartment: boolean = false;
  generalConfig?: GeneralConfig;

  globalRanks: Rank[] = [];
  departmentRanksPage?: Page<FormattedRank>;
  ranksButtons: Button[] = [];
  ranksCollation: Collation<RankPageCollationFilters> = {
    Page: 1,
    Filters: {
      Global: false,
    },
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
    private readonly _userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _eventService: EventService,
    private readonly _configService: ConfigService,
    private readonly _rankService: RankService,
    private readonly _rankOptions: RankOptions,
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

  tryEditMemberCallsignFormat(): void {
    this._modalService.open(DepartmentCallsignFormatEditorComponent, {
      title: 'Edit member callsign format',
    }, new Map().set(CALLSIGN_FORMAT_ITEMS, this.department!.CallsignFormat))
      .result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._departmentService.editCallsignFormat({
          DepartmentId: this.department!.Id,
          CallsignFormat: res.value,
        })),
        this._modalService.handleResponse()
      )
      .subscribe(() => this.getDepartment());
  }

  private getGeneralConfig(): void {
    this._configService.getGeneralConfig().subscribe(generalConfig => {
      this.generalConfig = generalConfig;

      this.ranksColumns = this._rankOptions.createColumns({
        generalConfig,
        showActionsColumn: this.mode === 'edit' && this.userService.userIsDepartmentManagerOrAdmin(),
        onEditSuccess: (res, rank) => {
          this._rankService.edit({
            DepartmentId: this.department?.Id,
            RankId: rank.Id,
            Name: res.value!['Name'],
            Permission: {
              DepartmentManager: res.value['DepartmentManager'],
              Kick: res.value['Kick'],
              SetRank: res.value['SetRank'],
              SetDivision: res.value['SetDivision'],
            },
            RequiredXp: res.value!['RequiredXp'],
          }).pipe(
            this._modalService.handleResponse()
          ).subscribe(() => this.getRanks(true))
        },
        onDeleteSuccess: () => this.getRanks(true),
      });

      this.ranksButtons = this._rankOptions.createButtons({
        generalConfig,
        showCreateButton: this.mode === 'edit' && this.userService.userIsDepartmentManagerOrAdmin(),
        onCreateSuccess: res => {
          this._rankService.create({
            DepartmentId: this.department!.Id,
            Name: res.value['Name'],
            Permission: {
              DepartmentManager: res.value['DepartmentManager'],
              Kick: res.value['Kick'],
              SetRank: res.value['SetRank'],
              SetDivision: res.value['SetDivision'],
            }
          }).pipe(
            this._modalService.handleResponse()
          ).subscribe(() => this.getRanks(true));
        }
      });

      this.divisionsColumns = this.createDivisionsColumns();
      this.divisionsButtons = this.createDivisionsButtons();
      this.usersColumns = this.createUsersColumns();
    });
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

    if (this.mode === 'edit' && this.userService.userIsDepartmentManagerOrAdmin()) {
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
                  new TextField({
                    key: 'Name',
                    label: 'UI.LABEL.NAME.SINGULAR',
                    placeholder: 'UI.LABEL.NAME.SINGULAR',
                    required: true,
                    value: updatedDivision?.Name,
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

    if (this.mode === 'edit' && this.userService.userIsDepartmentManagerOrAdmin()) {
      buttons.push({
        label: this._localization.translate('UI.ACTION.CREATE_DIVISION'),
        show: () => true,
        disabled: () => false,
        onClick: () => {
          this._modalFormService.open({
            title: 'UI.ACTION.CREATE_DIVISION',
            fields: () => [
              new TextField({
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

    if (this.userService.userIsDepartmentManagerOrAdmin()) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.CALLSIGN.TITLE'),
        accessorKey: 'Callsign',
        cellAction: {
          icon: IconType.Pencil,
          onClick: user => {
            let updatedCallsign = '';
            this._modalService.open(CallsignEditorComponent, {
                title: 'Edit callsign',
              }, new Map()
                .set(CALLSIGN_FORMAT_ITEMS, this.department?.CallsignFormat)
                .set(CALLSIGN_FORMAT_VALUES, user.CallsignFormat.Values)
            ).result
              .pipe(
                filter((res) => res.success),
                switchMap((res) => this.userService.updateUserDetails({Id: user.Id, CallsignFormatValues: res.value['CallsignFormatValues']})),
                this._modalService.handleResponse(),
              ).subscribe(res => {
              if (res.Status) {
                user.Callsign = updatedCallsign;
                if (this.user?.User.Id === user.Id) {
                  this._userService.refreshData();
                }
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

    if (this.userService.userIsDepartmentManagerOrAdmin() || this.user?.User?.Rank?.Permission.SetRank) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.RANK.SINGULAR'),
        actions: [
          {
            type: 'singleSelect',
            label: this._localization.translate('UI.LABEL.NO_RANK'),
            value: user => user?.RankId ?? -1,
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

    if (this.userService.userIsDepartmentManagerOrAdmin() || this.user?.User?.Rank?.Permission.SetDivision) {
      columns.push(new Column<FormattedUser>({
        header: this._localization.translate('UI.LABEL.DIVISION.SINGULAR'),
        actions: [
          {
            type: 'singleSelect',
            label: this._localization.translate('UI.LABEL.NO_DIVISION'),
            value: user => user?.DivisionId ?? -1,
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
        show: () => this.userService.userIsDepartmentManagerOrAdmin(),
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

    if (this.userService.userIsDepartmentManagerOrAdmin() || this.user?.User?.Rank?.Permission.Kick) {
      columns.push(new Column<FormattedUser>({
        header: 'Actions',
        actions: [
          {
            type: 'button',
            label: this._localization.translate('UI.ACTION.KICK'),
            disabled: user => user ? user.Id === this.user?.User.Id : false,
            onClick: user => {
              this._modalConfirmService.open({
                title: 'UI.ACTION.KICK',
                description: this._localization.translate('UI.LABEL.DEPARTMENT.KICK_CONFIRMATION'),
                buttons: {
                  submit: {variant: 'danger', label: 'UI.ACTION.KICK'},
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
    if (!this.department) {
      return [];
    }

    if (this.department.UseGlobalRanks) {
      return this.globalRanks.map(rank => {
        return {
          key: rank.Id,
          label: rank.Name,
        };
      });
    }

    return this.department?.DepartmentRanks?.map(rank => {
      return {
        key: rank.RankId,
        label: rank.Rank.Name,
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

    this._rankService.getPage({
      ...this.ranksCollation,
      Filters: {...this.ranksCollation.Filters, DepartmentId: this.department.Id}
    }).pipe(
      map(page => this._rankOptions.mapRanksToFormattedRanks(page))
    ).subscribe(page => {
      this.departmentRanksPage = page;

      if (remapUsers) {
        this.getDepartment();
      }
    });

    this._rankService.getAllGlobal().subscribe(res => this.globalRanks = res);
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
          RankName: this.ranksDropdown.find(rank => rank.key === user.RankId)?.label ?? '-',
          DivisionName: this.department?.Divisions?.find(division => division.Id === user.DivisionId)?.Name ?? '-',
        };
      });
      this.users = res as Page<FormattedUser>;
    });
  }

  onRanksCollationChange(collation: Collation): void {
    this.ranksCollation = collation as Collation<RankPageCollationFilters>;
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

  private createDepartmentSettingsForm(): void {
    if (!this.department) {
      return;
    }

    this.departmentSettingsForm = [
      new TextField({
        key: 'Name',
        required: true,
        label: 'UI.LABEL.NAME.SINGULAR',
        asyncValidators: [CustomValidators.nameShouldNotBeTaken(value => this._departmentService.checkIfDepartmentNameTaken(value, this.department!.Id, true))],
        value: this.department.Name,
      }),
      new NumberField({
        key: 'MaxUsers',
        label: 'UI.LABEL.MAX_MEMBERS.TITLE',
        required: true,
        value: this.department.MaxUsers,
        helpLabel: 'UI.LABEL.MAX_MEMBERS.HELP_LABEL',
      }),
      new SingleSelectField({
        key: 'DefaultRankId',
        label: 'UI.LABEL.DEFAULT_RANK.TITLE',
        required: false,
        helpLabel: 'UI.LABEL.DEFAULT_RANK.DESCRIPTION',
        defaultItemLabel: this._localization.translate('UI.LABEL.NO_DEFAULT_RANK'),
        value: this.department?.DefaultRankId ?? -1,
        items: () => this.ranksDropdown,
        size: 'normal',
        variant: 'secondary'
      }),
      new TextField({
        key: 'Callsign',
        label: 'UI.LABEL.CALLSIGN.TITLE',
        helpLabel: 'UI.LABEL.CALLSIGN.HELP_LABEL',
        validators: [CustomValidators.onlyLetters()],
        value: this.department.Callsign,
      }),
      new CheckboxField({
        key: 'UseGlobalRanks',
        label: 'Use global ranks',
        helpLabel: 'When enabled only the globally configured ranks will be available in this department.',
        value: this.department.UseGlobalRanks,
      }),
    ];
  }

  protected readonly UserRole = UserRole;
}
