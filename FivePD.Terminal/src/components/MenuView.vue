<script setup lang="ts">
import { type Ref, ref, onMounted, onBeforeMount } from 'vue';
import { useEmitter, useT } from '@hooks';
import { type Menu, MenuItem, MenuItemList, MenuItemToggle } from '@utils';
import { MenuItemType } from '@enums';
import { NuiEventType } from '@utils';

const props = defineProps<{
  menu: Menu
}>();

const emitter = useEmitter();
const t = useT();

const currentItemIndex: Ref<number> = ref(0);
const itemNodes: Ref<HTMLElement[] | null> = ref(null);
const selectorNode: Ref<HTMLElement | null> = ref(null);
const menuItems: Ref<MenuItem[]> = ref([]);

onBeforeMount(() => {
  menuItems.value = props.menu.items;
});

onMounted(() => {
  itemNodes.value = Array.from(document.querySelectorAll('.menu-item'));
  selectorNode.value = document.querySelector('.selector');

  console.log(props.menu)
});

emitter.on(NuiEventType.MenuUp, () => {
  if (!selectorNode.value || !itemNodes.value || !menuItems.value) return;

  if (currentItemIndex.value > 0) currentItemIndex.value = currentItemIndex.value - 1;
  else currentItemIndex.value = menuItems.value.length - 1;

  const currentItemNode = itemNodes.value[currentItemIndex.value];
  selectorNode.value.style.top = `${currentItemNode.offsetTop}px`;
});

emitter.on(NuiEventType.MenuDown, () => {
  if (!selectorNode.value || !itemNodes.value || !menuItems.value) return;

  if (currentItemIndex.value < menuItems.value.length - 1) currentItemIndex.value = currentItemIndex.value + 1;
  else currentItemIndex.value = 0;

  const currentItemNode = itemNodes.value[currentItemIndex.value];
  selectorNode.value.style.top = `${currentItemNode.offsetTop}px`;
});

emitter.on(NuiEventType.MenuLeft, () => onListSlide(NuiEventType.MenuLeft));
emitter.on(NuiEventType.MenuRight, () => onListSlide(NuiEventType.MenuRight));

emitter.on(NuiEventType.MenuSelect, () => {
  if (!selectorNode.value || !itemNodes.value || !menuItems.value) return;

  // Change DOM state
  selectorNode.value.classList.add('selector--on-select');
  setTimeout(() => {
    selectorNode.value!.classList.remove('selector--on-select');
  }, 100);

  // Change actual item state
  const currentItem = menuItems.value[currentItemIndex.value];
  if (currentItem.type === MenuItemType.Toggle) (currentItem as MenuItemToggle).toggle();
  currentItem.onPress();
});

const getMarqueeClass = (item: MenuItemList, title: string, listIndex: number) => {
  if (title.length <= 10) return '';
  return item.currentIndex === listIndex ? 'marquee' : '';
}

const onListSlide = (dir: string) => {
  if (!selectorNode.value || !itemNodes.value || !menuItems.value) return;

  const currentItem: MenuItemList = menuItems.value[currentItemIndex.value] as MenuItemList;
  if (currentItem.type !== MenuItemType.List) return;

  const currentItemNode = itemNodes.value[currentItemIndex.value];
  const leftArrow = currentItemNode.querySelector('.left-arrow');
  const rightArrow = currentItemNode.querySelector('.right-arrow');
  const wrapper = currentItemNode.querySelector('.list-wrapper .list-wrapper-inner');

  const arrow = dir === NuiEventType.MenuLeft ? leftArrow : rightArrow;
  arrow!.classList.add('on-press');
  setTimeout(() => {
    arrow!.classList.remove('on-press');
  }, 100);

  if (dir === NuiEventType.MenuLeft) {
    wrapper!.scrollLeft -= 100;
    currentItem.prev();
  } else {
    wrapper!.scrollLeft += 100;
    currentItem.next();
  }

}
</script>

<template>
  <div id="main">
    <div class="menu">
      <div class="menu-header">
        <p>{{ menu.useLocalization ? t(menu.title) : menu.title }}</p>
      </div>
      <div class="menu-items">

        <div class="selector"></div>

        <div v-for="(item, index) in menuItems || []" :key="index" class="menu-item">

          <p class="title">{{ menu.useLocalization ? t(item.title) : item.title }}</p>

          <div v-if="item.type === MenuItemType.Toggle" :class="`menu-item-toggle ${(item as MenuItemToggle).checked ? 'checked' : ''}`">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
            </svg>
          </div>

          <div v-if="item.type === MenuItemType.List" class="menu-item-list">
            <svg xmlns="http://www.w3.org/2000/svg" :class="`left-arrow ${(item as MenuItemList).currentIndex === 0 ? 'arrow-disabled' : ''}`" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
            </svg>
            <div class="list-wrapper">
              <div class="list-wrapper-inner">
                <div v-for="(listItem, listIndex) in (item as (MenuItemList)).items" :key="listIndex" class="list-wrapper-item">
                  <div :class="`list-wrapper-item-inner ${getMarqueeClass(item as MenuItemList, listItem, listIndex)}`">
                    <p>{{ listItem }}</p>
                  </div>
                </div>
              </div>
            </div>
            <svg xmlns="http://www.w3.org/2000/svg" :class="`right-arrow ${(item as MenuItemList).currentIndex === (item as MenuItemList).items.length-1 ? 'arrow-disabled' : ''}`" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
            </svg>
          </div>

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
        &.selector--on-select
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
        justify-content: space-between
        gap: 10px
        transition: var(--transition)
        margin-bottom: 5px
        padding: 0 10px

        .title
          color: var(--clr-text-secondary)

      .menu-item-toggle
        width: 20px
        height: 20px
        border: 2px solid var(--clr-bg-80)
        border-radius: var(--radius-sm)
        display: flex
        align-items: center
        justify-content: center
        transition: none
        &.checked
          border-color: var(--clr-text-secondary)

          svg
            opacity: 1

        svg
          height: 18px
          opacity: 0
          color: white
          transition: none

      .menu-item-list
        margin-right: -10px
        position: relative
        height: 100%
        width: calc(30px + 100px + 30px)
        display: grid
        grid: 1fr / 30px 1fr 30px
        align-items: center
        justify-items: center

        svg
          height: 22px
          color: var(--clr-text-secondary)
          transition: none
          &.arrow-disabled
            color: var(--clr-bg-80)
          &:not(.arrow-disabled).on-press
            color: var(--clr-blue)

        .list-wrapper
          width: 100%
          height: 100%
          overflow: hidden

          .list-wrapper-inner
            width: 100%
            height: 100%
            overflow: hidden
            display: flex
            flex-direction: row
            align-items: center
            overflow-y: hidden

            .list-wrapper-item
              position: relative
              width: 100px
              min-width: 100px
              height: 100%
              display: flex
              align-items: center
              justify-content: center
              overflow: hidden

              .list-wrapper-item-inner
                position: absolute
                z-index: 1
                top: 50%
                left: 50%
                transform: translate(-50%, -50%)
                display: flex
                flex-direction: row
                align-items: center
                &.marquee
                  left: 0
                  transform: translateY(-50%)
                  animation: marquee 4s linear infinite
                  animation-delay: 1s

                p
                  color: var(--clr-text-secondary)
                  white-space: nowrap

@keyframes marquee
  0%
    transform: translate(0, -50%)
  100%
    transform: translate(-50%, -50%)

</style>
