<script setup lang="ts">
import { ClientEvents } from '@enums';
import { IDepartment } from '@interfaces';
import { useState } from '@store';
import { post } from '@utils';
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';

const state = useState();
const route = useRoute();
const department = ref<IDepartment>();
const title = computed(() => `${state.global.localization.Nui.Labels.Department} ${(department?.value?.Name ? '(' + department?.value?.Name + ')' : '')}`);

onMounted(() => {
  if (state.user.details.Department?.Id === parseInt(route.params.id as string)) {
    department.value = state.user.details.Department;
  }
})

post<IDepartment>(ClientEvents.GetDepartmentById, parseInt(route.params.id as string)).subscribe((res) => {
  department.value = res;
});
</script>

<template>
  <PageHeader :title="title" />

  <Table
    title="Members"
    :rows="department?.Users ?? []"
    :columns="{
      Name: state.global.localization.Nui.Labels.Name,
      RankId: {
        replaceKeyFrom: department?.Ranks ?? [],
        label: state.global.localization.Nui.Labels.Rank,
      },
      DivisionId: {
        replaceKeyFrom: department?.Divisions ?? [],
        label: state.global.localization.Nui.Labels.Division
      },
    }"
  />

</template>

<style scoped lang="sass">
</style>