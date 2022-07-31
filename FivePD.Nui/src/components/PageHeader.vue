<script setup lang="ts">
import { PropButton } from '@interfaces';

const props = defineProps<{
  title: string,
  centerTitle?: boolean,
  buttons?: PropButton[],
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
    <div class="buttons" v-if="buttons">
      <Button v-for="button in buttons" :show="button.show" :onClick="button.onClick" :link="button.link" :label="button.label" theme="secondary" />
    </div>
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
  z-index: var(--layer-100)
  border-bottom: 1px solid var(--clr-bg-40)
  margin-bottom: 40px
  &.page-header--center-title
    justify-content: center
    border: none

    h1
      text-align: center

  h1
    font-size: 24px
    font-weight: 700
    color: var(--clr-text)

  .buttons
    display: flex
    flex-direction: row
    align-items: center
    justify-content: space-between

</style>
