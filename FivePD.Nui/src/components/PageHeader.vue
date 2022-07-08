<script setup lang="ts">
const props = defineProps<{
  title: string,
  centerTitle?: boolean,
}>();
</script>

<template>
  <div
    class="page-header"
    :class="{
      'page-header--center-title': centerTitle
    }"
  >
    <h1>{{ title }}</h1>
  </div>
</template>

<style scoped lang="sass">
.page-header
  position: sticky
  top: 0
  height: var(--page-header-height)
  display: flex
  flex-direction: row
  align-items: center
  justify-content: space-between
  background: var(--clr-bg)
  z-index: var(--layer-10)
  &.page-header--center-title
    justify-content: center

    h1
      text-align: center

  h1
    font-size: 24px
    font-weight: 700
    color: var(--clr-text)

</style>
