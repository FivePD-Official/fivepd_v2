<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();
const props = withDefaults(defineProps<{
  label: string,
  theme: 'primary' | 'secondary' | 'small' | 'danger',
  type?: 'button' | 'submit',
  onClick?: () => void,
  link?: string,
  show?: boolean,
}>(), {
  show: true,
  type: 'button',
});

function internalOnClick() {
  if (props.link) {
    router.push(props.link);
  } else if (props.onClick) {
    props.onClick();
  }
}
</script>

<template>
  <button
    :class="{
      'button--primary': theme === 'primary',
      'button--secondary': theme === 'secondary',
      'button--small': theme === 'small',
      'button--danger': theme === 'danger',
    }"
    :type="type"
    @click="internalOnClick"
    v-if="show"
  >
    <span>{{ label }}</span>
  </button>
</template>

<style scoped lang="sass">
button
  position: relative
  width: 100%
  height: 40px
  border: none
  border-radius: var(--radius-sm)
  font-size: 18px
  cursor: pointer
  background: transparent
  padding: 0 25px
  display: flex
  flex-direction: row
  align-items: center
  justify-content: center
  white-space: nowrap
  &.button--primary
    &::before
      content: ''
      position: absolute
      top: 0
      left: 0
      width: 100%
      height: 100%
      border-radius: var(--radius-sm)
      opacity: 1
      background: var(--clr-blue)
      z-index: var(--layer-0)
    &:hover
      &::before
        opacity: .8
  &.button--danger
    &::before
      content: ''
      position: absolute
      top: 0
      left: 0
      width: 100%
      height: 100%
      border-radius: var(--radius-sm)
      opacity: 1
      background: var(--clr-red)
      z-index: var(--layer-0)
    &:hover
      &::before
        opacity: .8

  &.button--secondary
    border: 2px solid var(--clr-text-tertiary)
    &:hover
      border-color: var(--clr-text-secondary)
  
  &.button--small
    height: 35px
    font-size: 16px
    border: 2px solid var(--clr-bg-80)
    &:hover
      border-color: var(--clr-text-tertiary)

    span
      color: var(--clr-text-secondary)

  span
    position: relative
    z-index: var(--layer-10)
    color: var(--clr-text-primary)
</style>