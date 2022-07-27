<script setup lang="ts">
import { ClientEvents } from '@enums';
import type { IDepartment, IRank, ColumnDef, IUser, ActionResponse, FormFilter, Page } from '@interfaces';
import { useState } from '@store';
import { post } from '@utils';
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { BaseRequest } from "@interfaces";
import { Observable } from "rxjs";

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

watch(route, () => {
  requestDepartmentData();
}, {
  deep: true
});

const ranksColumns: ColumnDef<FormattedRank> = [
  {
    header: state.global.localization.Nui.Labels.Name,
    accessorKey: 'Name',
    filter: true,
  },
  {
    header: state.global.localization.Nui.Labels.Permissions,
    accessorKey: 'Permissions',
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
  },
  {
    header: state.global.localization.Nui.Labels.Division,
    accessorKey: 'DivisionName',
  },
];

function requestDepartmentData() {
  post<IDepartment>(ClientEvents.GetDepartmentById, defaultDepartmentId).subscribe((res) => {
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

  post<ActionResponse>(ClientEvents.JoinDepartment, type === 'join' ? department.value.Id : 0).subscribe((res) => {
    // TODO: success / error notification
    if (res.Status) {
      router.push('/');
    } else {

    }
  });
}
</script>

<template>
  <PageHeader
    :title="title"
    :buttons="[
      {
        label: state.global.localization.Nui.Actions.Join,
        onClick: () => joinOrLeaveDepartment('join'),
        show: state.user.details.DepartmentId !== department?.Id,
      },
      {
        label: state.global.localization.Nui.Actions.Leave,
        onClick: () => joinOrLeaveDepartment('leave'),
        show: state.user.details.DepartmentId === department?.Id,
      }
    ]"
  />

  <Table
    :title="state.global.localization.Nui.Labels.Ranks"
    :columns="ranksColumns"
    :defaultData="department?.Ranks ?? []"
    :dataRequest="requestDepartmentRanks"
    :dataMapper="mapDepartmentRanks"
  />

  <Table
    :title="state.global.localization.Nui.Labels.Members"
    :columns="usersColumnDef"
    :defaultData="department?.Users ?? []"
    :dataRequest="requestDepartmentUsers"
    :dataMapper="mapDepartmentUsers"
  />

</template>

<style scoped lang="sass">
</style>