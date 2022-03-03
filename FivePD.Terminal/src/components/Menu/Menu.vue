<script setup lang="ts">
import { type Ref, ref, onMounted } from 'vue';
import useEmitter from '../../hooks/useEmitter';
import { MenuItem } from '../../utils/MenuItem';
import { MenuItemType } from '../../enums';
import { NuiEventType } from '../../utils';

const props = defineProps<{
  title: string,
  items: MenuItem[]
}>();

const emitter = useEmitter();

const currentItemIndex: Ref<number> = ref(0);
const itemNodes: Ref<HTMLElement[] | null> = ref(null);
const selectorNode: Ref<HTMLElement | null> = ref(null);

emitter.on(NuiEventType.MenuUp, () => {
  if (!selectorNode.value || !itemNodes.value || !props.items) return;

  if (currentItemIndex.value > 0) currentItemIndex.value = currentItemIndex.value - 1;
  else currentItemIndex.value = props.items.length - 1;

  const currentItemNode = itemNodes.value[currentItemIndex.value];
  selectorNode.value.style.top = `${currentItemNode.offsetTop}px`;
});

emitter.on(NuiEventType.MenuDown, () => {
  if (!selectorNode.value || !itemNodes.value || !props.items) return;

  if (currentItemIndex.value < props.items.length - 1) currentItemIndex.value = currentItemIndex.value + 1;
  else currentItemIndex.value = 0;

  const currentItemNode = itemNodes.value[currentItemIndex.value];
  selectorNode.value.style.top = `${currentItemNode.offsetTop}px`;
});

emitter.on(NuiEventType.MenuSelect, () => {
  if (!selectorNode.value || !itemNodes.value || !props.items) return;

  // Change DOM state
  selectorNode.value.classList.add('selector--on-enter');
  setTimeout(() => {
    selectorNode.value!.classList.remove('selector--on-enter');
  }, 100);

  // Change actual item state
  const currentItem = props.items[currentItemIndex.value];
  currentItem.onPress();
});

onMounted(() => {
  itemNodes.value = Array.from(document.querySelectorAll('.menu-item'));
  selectorNode.value = document.querySelector('.selector');
});

const getItemClass = (type: MenuItemType) => {
  switch (type) {
    case MenuItemType.List: return 'menu-item--list';
    case MenuItemType.Toggle: return 'menu-item--toggle';
  }
}
</script>

<template>
  <div id="main">
    <div class="menu">
      <div class="menu-header">
        <p>{{ title }}</p>
      </div>
      <div class="menu-items">

        <div class="selector"></div>

        <div v-for="item in items || []" :key="item.title" :class="`menu-item ${getItemClass(item.type)}`">
          <p class="title">{{ item.title }}</p>
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
    padding-bottom: 20px

    .menu-header
      display: flex
      flex-direction: row
      align-items: center
      justify-content: flex-start
      padding: 20px 15px

      p
        font-weight: 600
        color: var(--clr-text)

    .menu-items
      position: relative
      width: 100%
      display: flex
      flex-direction: column
      align-items: flex-start
      padding: 0 5px

      .selector
        position: absolute
        z-index: 1
        top: 0
        left: 0
        width: 100%
        height: 40px
        border-left: 3px solid var(--clr-gray)
        border-right: 3px solid var(--clr-gray)
        transition: var(--transition), border-color 0.1s cubic-bezier(0.33, 1, 0.68, 1)
        &::before
          content: ''
          position: absolute
          z-index: 1
          top: 0
          left: 0
          width: 100%
          height: 100%
          background: white
          opacity: 0.05
          transition: var(--transition)
        &.selector--on-enter
          border-color: var(--clr-blue)
          &::before
            background: var(--clr-blue)

      .menu-item
        position: relative
        z-index: 2
        width: 100%
        height: 40px
        display: flex
        flex-direction: row
        align-items: center
        transition: var(--transition)
        margin-bottom: 5px
        padding: 0 10px

        .title
          color: var(--clr-text-secondary)

</style>
