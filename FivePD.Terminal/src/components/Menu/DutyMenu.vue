<script setup lang="ts">
import { type MenuItem, MenuItemList, MenuItemToggle } from '@utils';

const onDutyToggle = () => {
  console.log('toggled on duty');
}
const getLoadout = (obj?: MenuItem) => {
  console.log(obj)
}

const loadoutItem = new MenuItemList('Get loadout', 'Loadout.', getLoadout);
loadoutItem.setItems = ['really long item title test', 'item 2', 'item 3', 'item 4'];

const items: MenuItem[] = [
  new MenuItemToggle('On duty', 'Toggles your duty status.', onDutyToggle),
  loadoutItem
];
</script>

<template>
  <Menu title="Duty menu" :items="items" />
</template>

<style lang="sass">
</style>
