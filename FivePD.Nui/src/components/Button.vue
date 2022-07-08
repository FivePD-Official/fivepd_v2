<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();
const props = defineProps<{
  label: string,
  theme: 'primary' | 'secondary',
  type?: 'button' | 'submit',
  onClick?: () => void,
  link?: string,
}>();

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
    }"
    :type="type ?? 'button'"
    @click="internalOnClick"
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
  &.button--primary
    &::before
      content: ''
      position: absolute
      top: 0
      left: 0
      width: 100%
      height: 100%
      border-radius: var(--radius)
      opacity: 1
      background: var(--clr-blue)
      z-index: var(--layer-0)
    &:hover
      &::before
        opacity: .8
    &:hover
      background: var(--clr-blue-light)

  &.button--secondary
    border: 2px solid var(--clr-text-tertiary)
    &:hover
      border-color: var(--clr-text-secondary)

  span
    position: relative
    z-index: var(--layer-10)
    color: var(--clr-text-primary)
</style>