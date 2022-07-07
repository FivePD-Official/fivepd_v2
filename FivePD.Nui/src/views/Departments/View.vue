<script setup lang="ts">
import { ClientEvents } from '@enums';
import { IDepartment } from '@interfaces';
import { useState } from '@store';
import { post } from '@utils';
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';

const state = useState();
const route = useRoute();
const department = ref<IDepartment>();
const title = computed(() => `${state.global.localization.Nui.Labels.Department} ${(department?.value?.Name ? '(' + department?.value?.Name + ')' : '')}`);

post<IDepartment>(ClientEvents.GetDepartmentById, parseInt(route.params.id as string)).subscribe((res) => {
  department.value = res;
});
</script>

<template>
  <PageHeader :title="title" />

  <div>

  </div>
</template>

<style scoped lang="sass">
</style>