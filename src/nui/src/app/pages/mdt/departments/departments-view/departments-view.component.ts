import { Component, OnInit } from '@angular/core';
import { Button, Checkbox, CheckboxGroup, Collation, Column, Department, Page, Rank, TextInput, User } from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalizationService } from '@services/localization/localization.service';
import { UserState } from '@store/user.state';
import { Observable, tap } from 'rxjs';
import { Select } from '@ngxs/store';
import { replaceParams } from '@utils';
import { ModalService } from '@services/modal/modal.service';

interface FormattedRank extends Rank {
  Permissions: string;
}

@Component({
  selector: 'app-departments-view',
  templateUrl: './departments-view.component.html',
  styleUrls: ['./departments-view.component.sass']
})
export class DepartmentsViewComponent implements OnInit {

  user?: User;
  @Select(UserState.details) user$!: Observable<User>;

  department?: Department;

  rank: any;

  ranks?: Page<FormattedRank>;
  ranksButtons: Button[] = [
    {
      label: this._localization.translate('Nui.Actions.CreateRank'),
      show: () => this.user?.Admin ?? false,
      onClick: () => this._modal.open({
        title: 'Nui.Actions.CreateRank',
        fields: [
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
            theme: 'secondary',
            label: 'Nui.Actions.Cancel',
          },
          submit: {
            theme: 'primary',
            label: 'Nui.Actions.Create',
          },
        },
      }).onSuccess(res => {
        this._departmentService.createRank({
          DepartmentId: this.department!.Id,
          Name: res.value!['Name'],
          Permission: res.value!['Permission']
        }).subscribe(createRes => {
          this.getRanks();
        });
      })
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
      show: this.user?.Admin ?? false,
      actions: [
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Delete'),
          onClick: rank => {
            this._modal.open({
              title: 'Nui.Labels.DeleteRankTitle',
              description: replaceParams(this._localization.translate('Nui.Labels.DeleteRankDescription'), { 'rank': `<b>${rank.Name}</b>`}),
              buttons: {
                cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
                submit: { theme: 'danger', label: 'Nui.Actions.Delete' },
              },
            }).onSuccess(res => {
              this._departmentService.deleteRank(rank.Id).subscribe(deleteRes => {
                if (deleteRes.Status) {
                  this.getRanks();
                }
              });
            });
          }
        },
        {
          type: 'button',
          label: 'Nui.Actions.Edit',
          onClick: rank => {
            this._modal.open({
              title: 'Nui.Labels.EditRankTitle',
              fields: [
                new TextInput({
                  key: 'Name',
                  label: this._localization.translate('Nui.Labels.Name'),
                  placeholder: this._localization.translate('Nui.Labels.Name'),
                  required: true,
                  value: rank.Name,
                }),
                new CheckboxGroup({
                  key: 'Permission',
                  label: 'Nui.Labels.Permissions',
                  checkboxGroup: [
                    new Checkbox({ key: 'Kick', label: 'Kick label', value: rank.Permission.Kick }),
                    new Checkbox({ key: 'SetRank', label: 'Set rank label', value: rank.Permission.SetRank }),
                    new Checkbox({ key: 'SetCallsign', label: 'Set callsign label', value: rank.Permission.SetCallsign }),
                  ]
                }),
              ],
              buttons: {
                cancel: { theme: 'secondary', label: 'Nui.Actions.Cancel' },
                submit: { theme: 'primary', label: 'Nui.Actions.Save' },
              },
            }).onSuccess(res => {
              this._departmentService.editRank({
                DepartmentId: this.department?.Id,
                RankId: rank.Id,
                Name: res.value!['Name'],
                Permission: res.value!['Permission']
              }).subscribe();
            });
          }
        },
      ]
    })
  ];

  constructor(
    private _router: Router,
    private _route: ActivatedRoute,
    private _localization: LocalizationService,
    private _departmentService: DepartmentService,
    private _modal: ModalService
  ) { }

  ngOnInit(): void {
    this.user$.subscribe(user => this.user = user);
    this._route.params.subscribe(params => {
      this.getDepartment();
    });
  }

  get title(): string {
    return `${this._localization.translate('Nui.Labels.Department')} ${(this.department?.Name ? '(' + this.department?.Name + ')' : '')}`;
  }

  get isUserInDepartment(): boolean {
    return this.user?.DepartmentId === this.department?.Id;
  }

  getDepartment(): void {
    this._departmentService.getDepartment(this._route.snapshot.params['id']).subscribe(res => {
      this.department = res;
      this.getRanks();
    });
  }

  getRanks(): void {
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
      console.log(this.department)
      console.log(this.ranks)
    });
  }

  onRanksCollationChange(collation: Collation): void {
    this.ranksCollation = collation;
    this.getRanks();
  }

  joinDepartment(): void {
    this._departmentService.joinDepartment(this.department!.Id).subscribe(() => {
      // TODO: Get user data
    });
  }

  leaveDepartment(): void {
    this._departmentService.leaveDepartment(this.department!.Id).subscribe(res => {
      if (res.Status) {
        // TODO: Get user data
        this._router.navigate(['mdt', 'dashboard']);
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

}
