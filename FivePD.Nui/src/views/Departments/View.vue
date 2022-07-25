<script setup lang="ts">
import { ClientEvents } from '@enums';
import type { IDepartment, IRank, ColumnDef, IUser, ActionResponse } from '@interfaces';
import { useState } from '@store';
import { post } from '@utils';
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

interface FormattedDepartment extends IDepartment {
  Ranks?: (IRank & { Permissions: string })[],
  Users?: (IUser & { RankName: string, DivisionName: string })[],
}

const state = useState();
const route = useRoute();
const router = useRouter();
const department = ref<FormattedDepartment>();
const title = computed(() => `${state.global.localization.Nui.Labels.Department} ${(department?.value?.Name ? '(' + department?.value?.Name + ')' : '')}`);

onMounted(() => {
  if (state.user.details.Department?.Id === parseInt(route.params.id as string)) {
    department.value = mapDepartmentData(state.user.details.Department);
  }
  requestDepartmentData();
})

watch(route, () => {
  requestDepartmentData();
}, {
  deep: true
});


function requestDepartmentData() {
  post<IDepartment>(ClientEvents.GetDepartmentById, parseInt(route.params.id as string)).subscribe((res) => {
    department.value = mapDepartmentData(res);
    console.log(department.value)
  });
}

function mapDepartmentData(department: IDepartment): FormattedDepartment {
  department.Users = department.Users?.map(user => {
    return {
      ...user,
      RankName: department.Ranks?.find(rank => rank.Id === user.RankId)?.Name ?? '-',  
      DivisionName: department.Divisions?.find(division => division.Id === user.DivisionId)?.Name ?? '-',
    };
  });
  department.Ranks = department.Ranks?.map(rank => {
    return {
      ...rank,
      Permissions: rankPermissionResolver(rank)
    };
  });
  return department as FormattedDepartment;
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
    :data="department?.Ranks ?? []"
    :columns="[
        {
          header: state.global.localization.Nui.Labels.Name,
          accessorKey: 'Name',
        },
        {
          header: state.global.localization.Nui.Labels.Permissions,
          accessorKey: 'Permissions', 
        }
    ]"
  />

  <Table
    :title="state.global.localization.Nui.Labels.Members"
    :data="department?.Users ?? []"
    :columns="[
      {
        header: state.global.localization.Nui.Labels.Name,
        accessorKey: 'Name',
      },
      {
        header: state.global.localization.Nui.Labels.Rank,
        accessorKey: 'RankName',
      },
      {
        header: state.global.localization.Nui.Labels.Division,
        accessorKey: 'DivisionName'
      },
    ]"
  />

</template>

<style scoped lang="sass">
</style>