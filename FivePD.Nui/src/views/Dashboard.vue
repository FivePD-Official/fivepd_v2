<script setup lang="ts">
import { ClientEvents } from '@enums';
import { onMounted, ref } from 'vue';
import { post } from '@utils';
import type { BaseRequest, ColumnDef, IDepartment, IUser, Page } from '@interfaces';
import { State } from '@store';
import { useStore } from 'vuex';
import { Observable } from 'rxjs';

const store = useStore<State>();

const statistics = ref(); // TODO
const assignedCall = ref(); // TODO

onMounted(() => {
  post<IUser>(ClientEvents.GetPlayerDetails).subscribe((res) => {
    store.commit('setUserDetails', res);
  });
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

const departmentColumnDef: ColumnDef<IDepartment> = [
  {
    header: store.state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    linkResolver: (item) => `/departments/${item.Id}`,
    filter: true,
  },
  {
    header: store.state.global.localization.Nui.Labels.Callsign,
    accessorKey: 'Callsign',
    filter: true,
  },
  {
    header: store.state.global.localization.Nui.Labels.Capacity,
    accessorKey: 'UserCount',
    valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${store.state.global.localization.Nui.Labels.NoLimit}` : `${item.UserCount} / ${item.MaxUsers}`,
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
      <p class="common-title">
        {{ store.state.global.localization.Nui.Labels.MyDetails }}
      </p>
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
        <p v-if="!statistics" class="no-data">
          {{ store.state.global.localization.Nui.Labels.NoDataToDisplay }}
        </p>
      </div>
    </div>

    <div class="grid-item grid-item--call">
      <p class="common-title">
        {{ store.state.global.localization.Nui.Labels.AssignedCall }}
      </p>
      <div class="common-bg grid-item--content">
        <p v-if="!assignedCall" class="no-data">
          {{ store.state.global.localization.Nui.Labels.NoAssignedCall }}
        </p>
      </div>
    </div>
  </div>

  <Table
    :title="store.state.global.localization.Nui.Labels.ActiveCalls"
    :columns="activeCallsColumnDef"
  />

  <Table
    :title="store.state.global.localization.Nui.Labels.Departments"
    :buttons="[
      {
        label: store.state.global.localization.Nui.Labels.NewDepartment,
        link: '/departments/new',
        show: store.state.user.details?.Admin ?? false
      }
    ]"
    :data-request="requestDepartments"
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
