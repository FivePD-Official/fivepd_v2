<script setup lang="ts">
import { ClientEvents } from "@enums";
import { onMounted, ref } from "vue";
import { modalService, post, replaceParams, showNotification, requestAndCommitUserData } from "@utils";
import type { BaseRequest, ColumnDef, IDepartment, Page, PropButton } from "@interfaces";
import { State } from "@store";
import { useStore } from "vuex";
import { Observable } from "rxjs";
import { ActionResponse, PropButtonWithResponse } from "@interfaces";
import { useRouter } from "vue-router";
import { useT } from "@hooks";

const router = useRouter();
const t = useT();
const store = useStore<State>();

const statistics = ref<any>(); // TODO
const assignedCall = ref<any>(); // TODO

onMounted(() => {
  requestAndCommitUserData();
});

const activeCallsColumnDef: ColumnDef<any> = [
  {
    header: store.state.global.localization.Nui.Labels.Type,
    accessorKey: 'Type',
  },
  {
    header: store.state.global.localization.Nui.Labels.Address,
    accessorKey: 'Address',
  },
  {
    header: store.state.global.localization.Nui.Labels.Status,
    accessorKey: 'Status',
  },
  {
    header: store.state.global.localization.Nui.Labels.Units,
    accessorKey: 'Units',
  }
];

const departmentButtons: PropButtonWithResponse[] = [
  {
    label: store.state.global.localization.Nui.Actions.CreateDepartment,
    show: () => store.state.user.details.Admin,
    onClick: onResponse => {
      modalService.open({
        title: store.state.global.localization.Nui.Actions.CreateDepartment,
        inputs: [
          {
            name: 'Name',
            label: store.state.global.localization.Nui.Labels.Name,
            type: 'text',
            required: true,
          },
          {
            name: 'Callsign',
            label: store.state.global.localization.Nui.Labels.Callsign,
            type: 'text',
            required: false,
            helpLabel: store.state.global.localization.Nui.Labels.CallsignHelpLabel,
            validator: (value: string) => !new RegExp('^[a-zA-Z]+$').test(value),
          },
          {
            name: 'CallsignFormat',
            label: store.state.global.localization.Nui.Labels.CallsignFormat,
            type: 'text',
            required: true,
            helpLabel: `
            ${store.state.global.localization.Nui.Labels.CallsignFormatHelpLabel} ${store.state.global.localization.Nui.Labels.AvailableCharacters}: <br/>
            <table>
              <tr>
                <td><b>L</b><td>
                <td>(${store.state.global.localization.Nui.Labels.Letter})<td>
              </tr>
              <tr>
                <td><b>N</b><td>
                <td>(${store.state.global.localization.Nui.Labels.Number})<td>
              </tr>
              <tr>
                <td><b>-</b><td>
                <td>(${store.state.global.localization.Nui.Labels.Dash})<td>
              </tr>
            </table>
          `,
            validator: (value: string) => !new RegExp('^[NL0-9\-*]{1,10}$').test(value),
          },
          {
            name: 'MaxUsers',
            label: store.state.global.localization.Nui.Labels.MaxNumberOfMembers,
            type: 'number',
            required: true,
            defaultValue: () => 0,
            helpLabel: store.state.global.localization.Nui.Labels.MaxNumberOfMembersHelpLabel,
          }
        ],
        buttons: {
          cancel: {
            theme: 'secondary',
            label: store.state.global.localization.Nui.Actions.Cancel,
          },
          submit: {
            theme: 'primary',
            label: store.state.global.localization.Nui.Actions.Create,
          },
        },
      }).subscribe(res => {
        if (res.success) {
          post<ActionResponse>(ClientEvents.CreateDepartment, res.inputFields).subscribe(createRes => {
            if (createRes.Status) {
              router.push('/');
              onResponse(createRes);
            }

            showNotification({
              type: createRes.Status ? 'success' : 'error',
              title: t(createRes.Title),
              description: createRes.Status ? replaceParams(t(createRes.Description), createRes.Params) : t(createRes.Description),
            });
          });
        }
      })
    }
  }
];

const departmentColumnDef: ColumnDef<IDepartment> = [
  {
    header: store.state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    linkResolver: (item) => `/departments/${item.Id}`,
    filter: {
      label: store.state.global.localization.Nui.Labels.SearchByName,
    },
  },
  {
    header: store.state.global.localization.Nui.Labels.Callsign,
    accessorKey: 'Callsign',
    filter: {
      label: store.state.global.localization.Nui.Labels.SearchByCallsign,
    },
  },
  {
    header: store.state.global.localization.Nui.Labels.Capacity,
    accessorKey: 'UserCount',
    valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${store.state.global.localization.Nui.Labels.NoLimit}` : `${item.UserCount} / ${item.MaxUsers}`,
  },
  {
    header: store.state.global.localization.Nui.Labels.Actions,
    actions: [
      {
        type: 'button',
        label: store.state.global.localization.Nui.Actions.Delete,
        onClick: (department, onResponse) => {
          modalService.open({
            title: store.state.global.localization.Nui.Labels.DeleteDepartmentTitle,
            description: replaceParams(store.state.global.localization.Nui.Labels.DeleteDepartmentDescription, { 'department': `<b>${department.Name}</b>`}),
            buttons: {
              cancel: {
                theme: 'secondary',
                label: store.state.global.localization.Nui.Actions.Cancel,
              },
              submit: {
                theme: 'danger',
                label: store.state.global.localization.Nui.Actions.Delete,
              },
            },
          }).subscribe(res => {
            if (res.success) {
              post<ActionResponse>(ClientEvents.DeleteDepartment, {
                DepartmentId: department.Id,
              }).subscribe(deleteRes => {
                showNotification({
                  type: deleteRes.Status ? 'success' : 'error',
                  title: t(deleteRes.Title),
                  description: deleteRes.Status ? replaceParams(t(deleteRes.Description), deleteRes.Params) : t(deleteRes.Description),
                })
                onResponse(deleteRes);
                if (deleteRes.Status) {
                  requestAndCommitUserData();
                }
              });
            }
          })
        }
      },
    ]
  }
];

function requestDepartments(departmentsRequest: BaseRequest): Observable<Page<IDepartment>> {
  return post<Page<IDepartment>>(ClientEvents.ListDepartments, departmentsRequest);
}
</script>

<template>
  <PageHeader :title="store.state.global.localization.Nui.Labels.Dashboard" />

  <div class="grid">
    <div class="grid-item grid-item--details">
      <p class="common-title">{{ store.state.global.localization.Nui.Labels.MyDetails }}</p>
      <div class="common-bg grid-item--content">
        <ul>
          <li>
            <p>{{ store.state.global.localization.Nui.Labels.Department }}</p>
            <p>{{ store.state.user.details.Department?.Name ?? '-' }}</p>
          </li>
          <li>
            <p>{{ store.state.global.localization.Nui.Labels.Division }}</p>
            <p>{{ store.state.user.details.Division?.Name ?? '-' }}</p>
          </li>
          <li>
            <p>{{ store.state.global.localization.Nui.Labels.Rank }}</p>
            <p>{{ store.state.user.details.Rank?.Name ?? '-' }}</p>
          </li>
           <li>
            <p>{{ store.state.global.localization.Nui.Labels.Callsign }}</p>
            <p>{{ store.state.user.details.Callsign ?? '-' }}</p>
          </li>
        </ul>
      </div>
    </div>

    <div class="grid-item grid-item--statistics">
      <p class="common-title">
        {{ store.state.global.localization.Nui.Labels.Statistics }}
      </p>
      <div class="common-bg grid-item--content">
        <p class="no-data" v-if="!statistics">{{ store.state.global.localization.Nui.Labels.NoDataToDisplay }}</p>
      </div>
    </div>

    <div class="grid-item grid-item--call">
      <p class="common-title">
        {{ store.state.global.localization.Nui.Labels.AssignedCall }}
      </p>
      <div class="common-bg grid-item--content">
        <p class="no-data" v-if="!assignedCall">{{ store.state.global.localization.Nui.Labels.NoAssignedCall }}</p>
      </div>
    </div>
  </div>

  <Table
    :title="store.state.global.localization.Nui.Labels.ActiveCalls"
    :columns="activeCallsColumnDef"
  />

  <Table
    :title="store.state.global.localization.Nui.Labels.Departments"
    :buttons="departmentButtons"
    :dataRequest="requestDepartments"
    :columns="departmentColumnDef"
  />
</template>

<style scoped lang="sass">
.grid
  display: grid
  grid: 1fr / repeat(3, 1fr)
  gap: 40px
  margin-bottom: 40px

  .grid-item
    width: 100%
    height: 100%

    .grid-item--content
      position: relative
      display: flex
      width: 100%
      height: 240px
      margin-top: 15px
      border-radius: var(--radius)
      background-color: var(--clr-bg-secondary)
      padding: 20px

      .no-data
        position: absolute
        top: 50%
        left: 50%
        transform: translate(-50%,-50%)
        font-size: 20px
        font-weight: 500
        color: var(--clr-text-secondary)
        text-align: center
        max-width: 25ch

  .grid-item--details
    ul
      width: 100%
      height: 100%
      list-style: none
      padding: 0
      display: grid
      grid: repeat(2, 1fr) / repeat(2, 1fr)
      align-items: flex-start
      gap: 20px

      li
        p
          font-size: 18px
          font-weight: 500
          color: var(--clr-text-tertiary)
          margin-bottom: 5px

          + p
            font-size: 20px
            font-weight: 400
            color: var(--clr-text-primary)
</style>
