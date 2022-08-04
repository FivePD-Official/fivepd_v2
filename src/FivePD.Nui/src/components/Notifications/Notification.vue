<script setup lang="ts">
import type { Notification } from '@interfaces';

const props = defineProps<{
  notification: Notification,
  exit: boolean
}>();
</script>

<template>
  <div class="notification" :class="{ 'exit': exit, 'notification-error': notification.type === 'error', 'notification-success': notification.type === 'success' }">
    <div class="notification-header">
      <div class="icon icon-success" v-if="notification.type === 'success'">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
        </svg>
      </div>
      <div class="icon icon-error" v-if="notification.type === 'error'">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </div>
      <p class="title">{{ notification.title }}</p>
    </div>
    <p class="description" v-html="notification.description"></p>
  </div>
</template>

<style scoped lang="sass">
.notification
  position: relative
  background: var(--clr-bg-20)
  padding: 15px
  box-shadow: 0 4px 8px 0 rgba(0, 0, 0, .2)
  border-radius: var(--radius-lg)
  margin-bottom: 20px
  animation: slideIn .2s
  animation-fill-mode: forwards
  width: 350px
  &.exit
    animation: slideOut .2s
    animation-fill-mode: forwards

  .notification-header
    display: flex
    flex-direction: row
    align-items: center
    margin-bottom: 10px

    .icon
      position: relative
      display: flex
      flex-direction: row
      align-items: center
      justify-content: center
      margin-right: 10px
      padding: 5px
      &::before
        content: ''
        position: absolute
        top: 0
        left: 0
        width: 100%
        height: 100%
        border-radius: 50%
        opacity: .1
      &.icon-success
        &::before
          background: var(--clr-blue)

        svg
          color: var(--clr-blue)

      &.icon-error
        &::before
          background: var(--clr-red)

        svg
          color: var(--clr-red)

      svg
        height: 22px

    .title
      font-size: 18px
      font-weight: 700
      color: var(--clr-text-secondary)

  .description
    font-size: 16px
    color: var(--clr-text-secondary)
    
  &.notification-error
    .title
      color: var(--clr-red)

  &.notification-success
    .title
      color: var(--clr-blue)

@keyframes slideIn
  0%
    transform: translateX(100%)
  100%
    transform: translateX(0)

@keyframes slideOut
  0%
    transform: translateX(0)
  100%
    transform: translateX(100%)
</style>