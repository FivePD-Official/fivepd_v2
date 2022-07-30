<script setup lang="ts">
import { onMounted, ref } from "vue";

export interface SelectItem {
  id: number;
  label: string;
  selected?: boolean;
}

const props = defineProps<{
  defaultItem: Omit<SelectItem, 'selected'>;
  items: SelectItem[];
  selectedItemResolver: (item: SelectItem) => boolean;
  onSelect: (item: SelectItem) => void;
  defaultSelectItemId?: number;
}>();

const selectedItem = ref(props.defaultItem);
const open = ref(false);
const filteredItems = ref(props.items);

onMounted(() => {
  for (const item of props.items) {
    const selected = props.selectedItemResolver(item);
    if (selected) {
      setSelectedItem(item);
      break;
    }
  }
});

function onSelect(selectItem: SelectItem) {
  props.onSelect(selectItem);
  open.value = false;
  setSelectedItem(selectItem);
}

function setSelectedItem(selectItem: SelectItem) {
  selectedItem.value = selectItem;
  filteredItems.value = props.items.filter(item => item.id !== selectItem.id);
  
  if (selectItem.id !== props.defaultItem.id) {
    const copy = [...filteredItems.value];
    copy.unshift(props.defaultItem);
    filteredItems.value = copy;
  }
}
</script>

<template>
  <div class="select" :class="{ 'select--open': open }" v-click-outside="() => open = false">
    <div class="selected" @click="open = !open">
      <p>{{ selectedItem.label }}</p>
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
      </svg>
    </div>
    <div class="items">
      <button class="item" v-for="item in filteredItems" @click="onSelect(item)">
        <p>{{ item.label }}</p>
      </button>
    </div>
  </div>
</template>

<style scoped lang="sass">
.select
  height: 35px
  
  .selected
    height: 35px
    border-radius: var(--radius-sm)
    padding: 0 15px
    display: flex
    flex-direction: row
    align-items: center
    justify-content: space-between
    background: var(--clr-bg-20)
    cursor: pointer
    &:hover
      background: var(--clr-bg-40)
    
    p
      font-size: 16px
      color: var(--clr-text-secondary)
      
    svg
      height: 20px
      margin-left: 15px
      color: var(--clr-text-secondary)
    
  .items
    display: none
    flex-direction: column
    align-items: center
    width: 100%
    box-shadow: 0 4px 8px 0 rgba(0, 0, 0, .2)
    
    .item
      width: 100%
      height: 35px
      padding: 0 15px
      display: flex
      flex-direction: row
      align-items: center
      justify-content: flex-start
      background: var(--clr-bg-20)
      background: var(--clr-bg-20)
      border: none
      outline: none
      cursor: pointer
      &:last-of-type
        border-bottom-left-radius: var(--radius-sm)
        border-bottom-right-radius: var(--radius-sm)
      &:hover
        background: var(--clr-bg-40)

      p
        font-size: 16px
        color: var(--clr-text-secondary)
      
  &.select--open
    .selected
      border-bottom-left-radius: 0
      border-bottom-right-radius: 0
      
      svg
        transform: rotate(180deg)
      
    .items
      display: flex
    
</style>