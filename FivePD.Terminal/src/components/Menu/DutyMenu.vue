<script setup lang="ts">
import { MenuItemList, MenuItemToggle, NuiEvents, Post } from '@utils';

const onDutyToggle = (obj?: MenuItemToggle) => {
  if (!obj) return;
}
const getLoadout = async (obj?: MenuItemList) => {
  if (!obj) return;

  const res = await Post(NuiEvents.GetLoadout).catch(() => {});
  console.log(res);
}

const dutyToggle = new MenuItemToggle('On duty', 'Toggles your duty status.', onDutyToggle);
const loadoutItem = new MenuItemList('Get loadout', 'Loadout.', getLoadout);
loadoutItem.setItems = ['really long item title test', 'item 2', 'item 3', 'item 4'];
</script>

<template>
  <Menu title="Duty menu" :items="[dutyToggle, loadoutItem]" />
</template>

<style lang="sass">
</style>
