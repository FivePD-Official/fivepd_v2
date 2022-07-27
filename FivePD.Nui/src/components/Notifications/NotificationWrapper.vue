<script setup lang="ts">
import { onNotification } from "@utils";
import { onMounted, onUnmounted, ref } from "vue";
import type { Notification } from "@interfaces";

type NotificationWithTimer = Notification & { timer: number; exit: boolean };

const intervalId = ref<NodeJS.Timer>();
const notifications = ref<NotificationWithTimer[]>([]);

onMounted(() => {
  intervalId.value = setInterval(() => {
    const notificationsCopy = [...notifications.value];
    notificationsCopy.forEach((notification) => {
      if (notification.timer === 4) {
        notification.exit = true;
      } else {
        notification.timer++;
      }
    });
    notifications.value = notificationsCopy.filter((x) => x.timer !== 5);
  }, 1000);
});

onUnmounted(() => {
  if (intervalId.value) {
    clearInterval(intervalId.value);
  }
});

onNotification().subscribe((notification) => {
  notifications.value.push({
    ...notification,
    timer: 0,
    exit: false,
  });
});
</script>

<template>
  <div class="wrapper">
    <Notification v-for="notification in notifications" :notification="notification" :exit="notification.exit" />
  </div>
</template>

<style scoped lang="sass">
.wrapper
  position: absolute
  bottom: 20px
  right: 40px
  overflow: hidden
  z-index: var(--layer-100)
</style>
