<script setup lang="ts">
import { ClientEvents } from '@enums';
import type { ActionResponse, ColumnDef, IDepartment, IDivision, IRank, IUser, Page, PropButton } from '@interfaces';
import { BaseRequest } from "@interfaces";
import { useState } from '@store';
import { FormFields, modalService, post, replaceParams, showNotification, requestAndCommitUserData } from '@utils';
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Observable } from "rxjs";
import { useT } from '@hooks';

interface FormattedRank extends IRank {
  Permissions: string;
}

interface FormattedUser extends IUser {
  RankName: string;
  DivisionName: string;
}

interface FormattedDepartment extends IDepartment {
  Ranks?: FormattedRank[];
  Users?: FormattedUser[];
}

const t = useT();
const state = useState();
const route = useRoute();
const router = useRouter();
const defaultDepartmentId = parseInt(route.params.id as string);
const department = ref<FormattedDepartment>();
const title = computed(() => `${state.global.localization.Nui.Labels.Department} ${(department?.value?.Name ? '(' + department?.value?.Name + ')' : '')}`);

onMounted(() => {
  if (state.user.details.Department?.Id === defaultDepartmentId) {
    department.value = mapDepartmentData(state.user.details.Department);
  }
  requestDepartmentData();
})

watch(route, (value) => {
  if (value.params.id) {
    requestDepartmentData(parseInt(value.params.id as string));
  }
}, {
  deep: true
});

const pageHeaderButtons: PropButton[] = [
  {
    label: state.global.localization.Nui.Actions.Join,
    onClick: () => joinOrLeaveDepartment('join'),
    show: () => state.user.details.DepartmentId !== department.value?.Id,
  },
  {
    label: state.global.localization.Nui.Actions.Leave,
    onClick: () => joinOrLeaveDepartment('leave'),
    show: () => state.user.details.DepartmentId === department.value?.Id,
  }
];

const ranksButtons: PropButton[] = [
  {
    label: state.global.localization.Nui.Actions.CreateRank,
    onClick: (onResponse) => {
      modalService.open({
        title: state.global.localization.Nui.Actions.CreateRank,
        inputs: [
          {
            type: 'text',
            name: 'Name',
            label: state.global.localization.Nui.Labels.Name,
            placeholder: state.global.localization.Nui.Labels.Name,
            required: true,
          },
          {
            type: 'checkboxGroup',
            name: 'Permission',
            label: state.global.localization.Nui.Labels.Permissions,
            required: false,
            checkboxes: [
              { id: 'Kick', label: 'Kick label' },
              { id: 'SetRank', label: 'Set rank label' },
              { id: 'SetCallsign', label: 'Set callsign label' },
            ]
          }
        ],
        buttons: {
          cancel: {
            theme: 'secondary',
            label: state.global.localization.Nui.Actions.Cancel,
          },
          submit: {
            theme: 'primary',
            label: state.global.localization.Nui.Actions.Create,
          },
        },
      }).subscribe(res => {
        if (res.success) {
          post<ActionResponse>(ClientEvents.CreateRank, {
            DepartmentId: department.value?.Id,
            Name: res.inputFields!['Name'],
            Permission: res.inputFields!['Permission']
          }).subscribe(createRes => {
            showNotification({
              type: createRes.Status ? 'success' : 'error',
              title: t(createRes.Title),
              description: createRes.Status ? replaceParams(t(createRes.Description), createRes.Params) : t(createRes.Description),
            })
            
            if (onResponse) {
              onResponse(createRes);
            }
          });
        }
      })
    },
    show: () => state.user.details.Admin,
  }
];

const ranksColumns: ColumnDef<FormattedRank> = [
  {
    header: state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    filter: true,
  },
  {
    header: state.global.localization.Nui.Labels.Permissions,
    accessorKey: 'Permissions',
  },
  {
    header: state.global.localization.Nui.Labels.Actions,
    show: () => state.user.details.Admin,
    actions: [
      {
        type: 'button',
        label: state.global.localization.Nui.Actions.Delete,
        onClick: (rank, onResponse) => {
          modalService.open({
            title: state.global.localization.Nui.Labels.DeleteRankTitle,
            description: replaceParams(state.global.localization.Nui.Labels.DeleteRankDescription, { 'rank': `<b>${rank.Name}</b>`}),
            buttons: {
              cancel: {
                theme: 'secondary',
                label: state.global.localization.Nui.Actions.Cancel,
              },
              submit: {
                theme: 'danger',
                label: state.global.localization.Nui.Actions.Delete,
              },
            },
          }).subscribe(res => {
            if (res.success) {
              post<ActionResponse>(ClientEvents.DeleteRank, {
                RankId: rank.Id
              }).subscribe(deleteRes => {
                showNotification({
                  type: deleteRes.Status ? 'success' : 'error',
                  title: t(deleteRes.Title),
                  description: deleteRes.Status ? replaceParams(t(deleteRes.Description), deleteRes.Params) : t(deleteRes.Description),
                })
                onResponse(deleteRes);
              });
            }
          })
        }
      },
      {
        type: 'button',
        label: state.global.localization.Nui.Actions.Edit,
        onClick: (rank, onResponse) => {
          modalService.open({
            title: state.global.localization.Nui.Labels.EditRankTitle,
            inputs: [
              {
                type: 'text',
                name: 'Name',
                label: state.global.localization.Nui.Labels.Name,
                placeholder: state.global.localization.Nui.Labels.Name,
                required: true,
                defaultValue: rank.Name,
              },
              {
                type: 'checkboxGroup',
                name: 'Permission',
                label: state.global.localization.Nui.Labels.Permissions,
                required: false,
                checkboxes: [
                  { id: 'Kick', label: state.global.localization.Nui.Actions.Kick, defaultValue: rank.Permission.Kick },
                  { id: 'SetRank', label: state.global.localization.Nui.Actions.SetRank, defaultValue: rank.Permission.SetRank },
                  { id: 'SetCallsign', label: state.global.localization.Nui.Actions.SetCallsign, defaultValue: rank.Permission.SetCallsign },
                ]
              }
            ],
            buttons: {
              cancel: {
                theme: 'secondary',
                label: state.global.localization.Nui.Actions.Cancel,
              },
              submit: {
                theme: 'primary',
                label: state.global.localization.Nui.Actions.Save,
              },
            },
          }).subscribe(res => {
            if (res.success) {
              post<ActionResponse>(ClientEvents.EditRank, {
                DepartmentId: department.value?.Id,
                RankId: rank.Id,
                Name: res.inputFields!['Name'],
                Permission: res.inputFields!['Permission']
              }).subscribe(editRes => {
                showNotification({
                  type: editRes.Status ? 'success' : 'error',
                  title: t(editRes.Title),
                  description: editRes.Status ? replaceParams(t(editRes.Description), editRes.Params) : t(editRes.Description),
                })
                onResponse(editRes);
              });
            }
          })
        }
      },
    ]
  }
];

const divisionsButtons: PropButton[] = [
  {
    label: state.global.localization.Nui.Actions.CreateDivision,
    onClick: (onResponse) => {
      modalService.open({
        title: state.global.localization.Nui.Actions.CreateDivision,
        inputs: [{
          type: 'text',
          name: 'Name',
          label: state.global.localization.Nui.Labels.Name,
          placeholder: state.global.localization.Nui.Labels.Name,
          required: true,
        }],
        buttons: {
          cancel: {
            theme: 'secondary',
            label: state.global.localization.Nui.Actions.Cancel,
          },
          submit: {
            theme: 'primary',
            label: state.global.localization.Nui.Actions.Create,
          },
        },
      }).subscribe(res => {
        if (res.success) {
          post<ActionResponse>(ClientEvents.CreateDivision, {
            DepartmentId: department.value?.Id,
            Name: res.inputFields!['Name']
          }).subscribe(createRes => {
            showNotification({
              type: createRes.Status ? 'success' : 'error',
              title: t(createRes.Title),
              description: createRes.Status ? replaceParams(t(createRes.Description), createRes.Params) : t(createRes.Description),
            })
            
            if (onResponse) {
              onResponse(createRes);
            }
          });
        }
      })
    },
    show: () => state.user.details.Admin,
  }
];

const divisionsColumns: ColumnDef<IDivision> = [
  {
    header: state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    filter: true,
  },
  {
    header: state.global.localization.Nui.Labels.Actions,
    show: () => state.user.details.Admin,
    actions: [
      {
        type: 'button',
        label: state.global.localization.Nui.Actions.Delete,
        onClick: (division, onResponse) => {
          modalService.open({
            title: state.global.localization.Nui.Labels.DeleteDivisionTitle,
            description: replaceParams(state.global.localization.Nui.Labels.DeleteDivisionDescription, { 'division': `<b>${division.Name}</b>`}),
            buttons: {
              cancel: {
                theme: 'secondary',
                label: state.global.localization.Nui.Actions.Cancel,
              },
              submit: {
                theme: 'danger',
                label: state.global.localization.Nui.Actions.Delete,
              },
            },
          }).subscribe(res => {
            if (res.success) {
              post<ActionResponse>(ClientEvents.DeleteDivision, {
                DepartmentId: department.value?.Id,
                DivisionId: division.Id,
              }).subscribe(deleteRes => {
                showNotification({
                  type: deleteRes.Status ? 'success' : 'error',
                  title: t(deleteRes.Title),
                  description: deleteRes.Status ? replaceParams(t(deleteRes.Description), deleteRes.Params) : t(deleteRes.Description),
                })
                onResponse(deleteRes);
              });
            }
          })
        }
      },
      {
        type: 'button',
        label: state.global.localization.Nui.Actions.Edit,
        onClick: (division, onResponse) => {
          modalService.open({
            title: state.global.localization.Nui.Labels.EditDivision,
            inputs: [{
              type: 'text',
              name: 'Name',
              label: state.global.localization.Nui.Labels.Name,
              placeholder: state.global.localization.Nui.Labels.Name,
              required: true,
              defaultValue: division.Name,
            }],
            buttons: {
              cancel: {
                theme: 'secondary',
                label: state.global.localization.Nui.Actions.Cancel,
              },
              submit: {
                theme: 'primary',
                label: state.global.localization.Nui.Actions.Save,
              },
            },
          }).subscribe(res => {
            if (res.success) {
              post<ActionResponse>(ClientEvents.EditDivision, {
                DivisionId: division.Id,
                Name: res.inputFields!['Name'],
              }).subscribe(renameRes => {
                showNotification({
                  type: renameRes.Status ? 'success' : 'error',
                  title: t(renameRes.Title),
                  description: renameRes.Status ? replaceParams(t(renameRes.Description), renameRes.Params) : t(renameRes.Description),
                })
                onResponse(renameRes);
              });
            }
          })
        }
      }
    ]
  }
];

const usersColumnDef: ColumnDef<FormattedUser> = [
  {
    header: state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    filter: true,
  },
  {
    header: state.global.localization.Nui.Labels.Rank,
    accessorKey: 'RankName',
    show: () => !state.user.details.Admin,
  },
  {
    header: state.global.localization.Nui.Labels.Division,
    accessorKey: 'DivisionName',
    show: () => !state.user.details.Admin,
  },
  {
    header: state.global.localization.Nui.Labels.Rank,
    show: () => state.user.details.Admin,
    actions: [
      {
        type: 'select',
        label: state.global.localization.Nui.Labels.NoRank,
        selectedItemResolver: (user, itemId) => user.RankId === itemId,
        items: () => department.value?.Ranks?.map(rank => {
          return {
            id: rank.Id,
            label: rank.Name,
          };
        }) ?? [],
        onSelect: (user, selectedId, onResponse) => {
          post<ActionResponse>(ClientEvents.ChangePlayerRank, {
            UserId: user.Id,
            IdToChange: selectedId,
          }).subscribe(deleteRes => {
            showNotification({
              type: deleteRes.Status ? 'success' : 'error',
              title: t(deleteRes.Title),
              description: deleteRes.Status ? replaceParams(t(deleteRes.Description), deleteRes.Params) : t(deleteRes.Description),
            })
            onResponse(deleteRes);
          });
        }
      }
    ]
  },
  {
    header: state.global.localization.Nui.Labels.Division,
    show: () => state.user.details.Admin,
    actions: [
      {
        type: 'select',
        label: state.global.localization.Nui.Labels.NoDivision,
        selectedItemResolver: (user, itemId) => user.DivisionId === itemId,
        items: () => department.value?.Divisions?.map(division => {
          return {
            id: division.Id,
            label: division.Name,
          };
        }) ?? [],
        onSelect: (user, selectedId, onResponse) => {
          post<ActionResponse>(ClientEvents.ChangePlayerDivision, {
            UserId: user.Id,
            IdToChange: selectedId,
          }).subscribe(deleteRes => {
            showNotification({
              type: deleteRes.Status ? 'success' : 'error',
              title: t(deleteRes.Title),
              description: deleteRes.Status ? replaceParams(t(deleteRes.Description), deleteRes.Params) : t(deleteRes.Description),
            })
            onResponse(deleteRes);
          });
        }
      },
    ]
  }
];

function requestDepartmentData(departmentId?: number) {
  post<IDepartment>(ClientEvents.GetDepartmentById, {
    DepartmentId: departmentId ?? defaultDepartmentId,
  }).subscribe((res) => {
    department.value = mapDepartmentData(res);
  });
}

function requestDepartmentUsers(userRequest: BaseRequest): Observable<Page<IUser>> {
  return post<Page<IUser>>(ClientEvents.GetDepartmentUsers, {
    DepartmentId: department.value?.Id ?? defaultDepartmentId,
    ...userRequest
  });
}

function requestDepartmentRanks(ranksRequest: BaseRequest): Observable<Page<IRank>> {
  return post<Page<IRank>>(ClientEvents.GetDepartmentRanks, {
    DepartmentId: department.value?.Id ?? defaultDepartmentId,
    ...ranksRequest
  });
}

function requestDepartmentDivisions(divisionsRequest: BaseRequest): Observable<Page<IDivision>> {
  return post<Page<IDivision>>(ClientEvents.GetDepartmentDivisions, {
    DepartmentId: department.value?.Id ?? defaultDepartmentId,
    ...divisionsRequest
  });
}

function mapDepartmentData(department: IDepartment): FormattedDepartment {
  return department as FormattedDepartment;
}

function mapDepartmentUsers(users: Page<IUser>): Page<IUser> {
  users.Items = users.Items.map(user => {
    return {
      ...user,
      RankName: department.value?.Ranks?.find(rank => rank.Id === user.RankId)?.Name ?? '-',
      DivisionName: department.value?.Divisions?.find(division => division.Id === user.DivisionId)?.Name ?? '-',
    };
  });
  return users;
}

function mapDepartmentRanks(ranks: Page<IRank>): Page<IRank> {
  ranks.Items = ranks.Items.map(rank => {
    return {
      ...rank,
      Permissions: rankPermissionResolver(rank)
    };
  }); 
  
  return ranks;
}

function rankPermissionResolver(rank: IRank): string {
  let permissions = '';

  if (rank.Permission.Kick) {
    permissions += `${state.global.localization.Nui.Actions.Kick}, `;
  }
  if (rank.Permission.SetRank) {
    permissions += `${state.global.localization.Nui.Actions.SetRank}, `;
  }
  if (rank.Permission.SetCallsign) {
    permissions += `${state.global.localization.Nui.Actions.SetCallsign}, `;
  }

  return permissions.length ? permissions.slice(0, -2) : '-';
}

function joinOrLeaveDepartment(type: 'join' | 'leave') {
  if (!department.value) {
    return;
  }

  post<ActionResponse>(type === 'join' ? ClientEvents.JoinDepartment : ClientEvents.LeaveDepartment, type === 'join' ? department.value.Id : 0).subscribe((res) => {
    if (res.Status) {
      showNotification({
        type: 'success',
        title: t(res.Title),
        description: replaceParams(t(res.Description), res.Params),
      });
      
      if (type === 'join') {
         requestAndCommitUserData();
      } else {
        router.push('/');
      }
    } else {
      showNotification({
        type: 'error',
        title: t(res.Title),
        description: t(res.Description),
      });
    }

  });
}

function onDepartmentSettingsSubmit(fields: FormFields) {
  post<ActionResponse>(ClientEvents.EditDepartment, {
    DepartmentId: department.value?.Id ?? defaultDepartmentId,
    ...fields
  }).subscribe(res => {
    showNotification({
      type: res.Status ? 'success' : 'error',
      title: t(res.Title),
      description: res.Status ? replaceParams(t(res.Description), res.Params) : t(res.Description),
    });
    
    if (res.Status) {
      requestDepartmentData();
    }
  });
}
</script>

<template>
  <PageHeader
    :title="title"
    :buttons="pageHeaderButtons"
  />
  
  <template v-if="state.user.details?.Admin">
    <Grid :columns="2" v-if="department">
      <EditableForm 
          :title="state.global.localization.Nui.Labels.DepartmentSettings"
          :submitButtonLabel="state.global.localization.Nui.Actions.Save"
          :onSubmit="onDepartmentSettingsSubmit"
          :fields="[
              [
                 {
                  name: 'Name',
                  label: state.global.localization.Nui.Labels.Name,
                  type: 'text',
                  required: true,
                  defaultValue: department.Name,
                },
                {
                  name: 'MaxUsers',
                  label: state.global.localization.Nui.Labels.MaxNumberOfMembers,
                  type: 'number',
                  required: true,
                  defaultValue: department.MaxUsers,
                  helpLabel: state.global.localization.Nui.Labels.MaxNumberOfMembersHelpLabel,
                }
              ],
              [
                {
                  name: 'Callsign',
                  label: state.global.localization.Nui.Labels.Callsign,
                  type: 'text',
                  required: false,
                  helpLabel: state.global.localization.Nui.Labels.CallsignHelpLabel,
                  defaultValue: department.Callsign,
                  validator: (value: string) => !new RegExp('^[a-zA-Z]+$').test(value),
                },
                {
                  name: 'CallsignFormat',
                  label: state.global.localization.Nui.Labels.CallsignFormat,
                  type: 'text',
                  required: true,
                  defaultValue: department.CallsignFormat,
                  helpLabel: `
                    ${state.global.localization.Nui.Labels.CallsignFormatHelpLabel} ${state.global.localization.Nui.Labels.AvailableCharacters}: <br/>
                    <table>
                      <tr>
                        <td><b>L</b><td>
                        <td>(${state.global.localization.Nui.Labels.Letter})<td>
                      </tr>
                      <tr>
                        <td><b>N</b><td>
                        <td>(${state.global.localization.Nui.Labels.Number})<td>
                      </tr>
                      <tr>
                        <td><b>-</b><td>
                        <td>(${state.global.localization.Nui.Labels.Dash})<td>
                      </tr>
                    </table>
                  `,
                  validator: (value: string) => !new RegExp('^[NL0-9\-*]{1,10}$').test(value),
                }
              ]
          ]"
      />
      <Table
          :title="state.global.localization.Nui.Labels.Divisions"
          :buttons="divisionsButtons"
          :columns="divisionsColumns"
          :dataRequest="requestDepartmentDivisions"
      />
    </Grid>
    <Table
        :title="state.global.localization.Nui.Labels.Ranks"
        :buttons="ranksButtons"
        :columns="ranksColumns"
        :dataRequest="requestDepartmentRanks"
        :dataMapper="mapDepartmentRanks"
    />
  </template>

  <template v-if="!state.user.details?.Admin">
    <Grid :columns="2" v-if="department">
      <Table
          :title="state.global.localization.Nui.Labels.Ranks"
          :buttons="ranksButtons"
          :columns="ranksColumns"
          :dataRequest="requestDepartmentRanks"
          :dataMapper="mapDepartmentRanks"
      />
      <Table
          :title="state.global.localization.Nui.Labels.Divisions"
          :buttons="divisionsButtons"
          :columns="divisionsColumns"
          :dataRequest="requestDepartmentDivisions"
      />
    </Grid>
  </template>

  <Table
    v-if="department"
    :title="state.global.localization.Nui.Labels.Members"
    :columns="usersColumnDef"
    :dataRequest="requestDepartmentUsers"
    :dataMapper="mapDepartmentUsers"
  />


</template>

<style scoped lang="sass">
</style>