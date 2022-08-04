<script setup lang="ts">
const props = defineProps<{
  rows?: number; // TODO?
  columns: number;
}>();
</script>

<template>
  <div class="grid" :style="{
    gridTemplateColumns: `repeat(${columns}, 1fr)`
  }">
    <slot></slot>
  </div>
</template>

<style scoped lang="sass">
.grid
  display: grid
  gap: 40px
  grid-auto-rows: 1fr
</style>