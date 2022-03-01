<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useStore } from 'vuex';

const props = defineProps<{
  title: string
}>();
const store = useStore();

const items: any = ref(null);
const selector: any = ref(null);

watch(store.state.menu, () => {
  if (!selector.value || !items.value) return;

  selector.value.style.top = `${items.value[store.state.menu.currentItemIndex].offsetTop}px`;

  if (store.state.menu.selectedItem) selector.value.classList.add('selector--focused');
  else selector.value.classList.remove('selector--focused');
});

onMounted(() => {
  items.value = Array.from(document.querySelectorAll('.menu-item'));
  selector.value = document.querySelector('.selector');

  store.commit('setItemCount', items.value.length);
})
</script>

<template>
  <div id="main">
    <div class="menu">
      <div class="menu-header">
        <p>{{ title }}</p>
      </div>
      <div class="menu-items">

        <div class="selector"></div>

        <div class="menu-item menu-item--checkbox">
          <div class="checkbox">
          </div>
          <p class="title">On Duty</p>
        </div>

        <div class="menu-item menu-item--checkbox menu-item--active">
          <div class="checkbox">
          </div>
          <p class="title">On Duty</p>
        </div>

        <div class="menu-item menu-item--checkbox">
          <div class="checkbox">
          </div>
          <p class="title">On Duty</p>
        </div>

      </div>
    </div>
  </div>
</template>

<style lang="sass">
#main
  position: relative
  z-index: 10
  max-width: 1900px
  width: 100%
  height: 100vh
  @media (max-width: 1920px)
    width: 95vw

  .menu
    position: absolute
    top: 50px
    background: var(--clr-bg)
    min-width: 300px
    border-radius: var(--radius)
    padding: 0 15px
    padding-bottom: 20px

    .menu-header
      display: flex
      flex-direction: row
      align-items: center
      justify-content: flex-start
      padding: 20px 0

      p
        font-weight: 600
        color: var(--clr-text)

    .menu-items
      position: relative
      width: 100%
      display: flex
      flex-direction: column
      align-items: flex-start

      .selector
        position: absolute
        top: 0
        left: 0
        width: 100%
        height: 40px
        border: 2px solid var(--clr-gray)
        border-radius: var(--radius)
        &.selector--focused
          border-color: var(--clr-blue)

      .menu-item
        width: 100%
        height: 40px
        display: flex
        flex-direction: row
        align-items: center
        transition: var(--transition)
        margin-bottom: 5px
        padding: 0 15px

        .title
          color: var(--clr-text-secondary)

</style>
