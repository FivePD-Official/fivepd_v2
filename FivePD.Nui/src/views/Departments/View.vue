<script setup lang="ts">
import { ClientEvents } from '@enums';
import type { IDepartment, IRank, Column, IUser } from '@interfaces';
import { useState } from '@store';
import { post } from '@utils';
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';

const state = useState();
const route = useRoute();
const department = ref<IDepartment>();
const title = computed(() => `${state.global.localization.Nui.Labels.Department} ${(department?.value?.Name ? '(' + department?.value?.Name + ')' : '')}`);

onMounted(() => {
  if (state.user.details.Department?.Id === parseInt(route.params.id as string)) {
    department.value = state.user.details.Department;
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
    department.value = res;
  });
}

function rowIdToNameResolver(rowItem: IUser, columnKey: string, columnValue: Column, substituteList: any[]) {
  return substituteList.find(sub => sub.Id === rowItem[columnKey])?.Name;
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
</script>

<template>
  <PageHeader :title="title" />

  <Table
    :title="state.global.localization.Nui.Labels.Ranks"
    :rows="department?.Ranks ?? []"
    :columns="{
      Name: {
        label: state.global.localization.Nui.Labels.Name,
      },
      Permissions: {
        valueResolver: (rowItem) => rankPermissionResolver(rowItem),
        label: state.global.localization.Nui.Labels.Permissions,
      }
    }"
  />

  <Table
    :title="state.global.localization.Nui.Labels.Members"
    :rows="department?.Users ?? []"
    :columns="{
      Name: {
        label: state.global.localization.Nui.Labels.Name,
      },
      RankId: {
        valueResolver: (rowItem: IUser, columnKey, columnValue) => rowIdToNameResolver(rowItem, columnKey, columnValue, department?.Ranks ?? []),
        label: state.global.localization.Nui.Labels.Rank,
      },
      DivisionId: {
        valueResolver: (rowItem, columnKey, columnValue) => rowIdToNameResolver(rowItem, columnKey, columnValue, department?.Divisions ?? []),
        label: state.global.localization.Nui.Labels.Division
      },
    }"
  />

</template>

<style scoped lang="sass">
</style>