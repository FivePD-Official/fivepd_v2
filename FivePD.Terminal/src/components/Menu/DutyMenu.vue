<script setup lang="ts">
import { MenuItem, MenuItemToggle } from '../../utils/MenuItem';

const onDutyToggle = () => {
  console.log('toggled on duty');
}
const onDutyToggle2 = () => {
  console.log('toggled on duty 2');
}

const items: MenuItem[] = [
  new MenuItemToggle('On duty', 'Toggles your duty status.', onDutyToggle),
  new MenuItemToggle('On duty 2', 'Toggles your duty status.', onDutyToggle2)
];
</script>

<template>
  <Menu title="Duty menu" :items="items" />
</template>

<style lang="sass">
</style>
