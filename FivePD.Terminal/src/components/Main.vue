<script setup lang="ts">
import { useStore } from 'vuex';

const store = useStore();

</script>

<template>
  <div id="main-overlay"></div>
  <div id="main">
    <Header />
    <div :class="`main-subgrid ${store.state.user.isSidebarCollapsed ? 'collapsed' : ''}`">
      <Sidebar />
      <div class="router-view">
        <router-view></router-view>
      </div>
    </div>
  </div>
</template>

<style scoped lang="sass">
#main-overlay
  position: fixed
  z-index: 0
  inset: 0
  width: 100vw
  height: 100vh
  background: var(--clr-overlay)
  opacity: .5

#main
  position: relative
  z-index: 10
  max-width: 1900px
  width: 100%
  height: 90vh
  background: var(--clr-bg)
  border-radius: var(--radius-lg)
  display: grid
  grid: 80px 1fr / 1fr
  @media (max-width: 1920px)
    width: 95vw

  .main-subgrid
    display: grid
    grid: 1fr / 250px 1fr
    overflow: hidden
    &.collapsed
      grid: 1fr / 90px 1fr

    .router-view
      width: 100%
      height: 100%
      overflow-x: hidden
      padding: 0 40px 40px 40px

</style>
