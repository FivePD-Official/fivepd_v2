<script setup lang="ts">
import { ClientEvents } from '@enums';
import type { ActionResponse, Column, IDepartment, IRank, IUser } from '@interfaces';
import { useState } from '@store';
import { post, replaceParams, showNotification, toSentence } from '@utils';
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useT } from "@hooks";

const state = useState();
const route = useRoute();
const router = useRouter();
const t = useT();
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

function joinOrLeaveDepartment(type: 'join' | 'leave') {
  if (!department.value) {
    return;
  }

  post<ActionResponse>(type === 'join' ? ClientEvents.JoinDepartment : ClientEvents.LeaveDepartment, type === 'join' ? department.value.Id : 0).subscribe((res) => {
    if (res.Status) {
      showNotification({
        type: 'success',
        title: type === 'join' ? state.global.localization.Nui.Labels.SuccessfulJoin : state.global.localization.Nui.Labels.SuccessfulLeave,
        description: replaceParams(t(res.Message), { 'department': `<b>${department.value?.Name ?? '-'}</b>` }),
      });
      router.push('/');
    } else {
      showNotification({
        type: 'error',
        title: state.global.localization.Nui.Errors.UnexpectedError,
        description: toSentence(
          t(res.Message),
          state.global.localization.Nui.Errors.PleaseTryAgainLater
        ),
      });
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