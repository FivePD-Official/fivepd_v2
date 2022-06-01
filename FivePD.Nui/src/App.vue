<script setup lang="ts">
import { hideNui, showNui, post, Menu, MenuItem, MenuItemButton, MenuItemList, MenuItemToggle, nuiEventNext, onNuiEvent } from '@utils';
import { MenuItemType, NuiEventType, NuiEvents } from '@enums';
import { useRouter } from 'vue-router';
import { ref } from 'vue';
import { useEmitter } from '@hooks';
import { useStore } from 'vuex';
import type { IBaseNuiEvent, ILocalization, LocalizationChangeResponse, MenuObjectsResponse, RadialMenuObjectsResponse, ViewEventResponse } from '@interfaces';

const store = useStore();
const emitter = useEmitter();
const router = useRouter();
const viewType = ref<string>('');
const currentMenuId = ref<string>('#');

window.addEventListener('load', () => {

  const isDev = import.meta.env.MODE === 'development';

  document.head.innerHTML = document.head.innerHTML + '<base href=' + (isDev ? '' : '/Libraries/FivePD.Terminal/') + '/>';
  document.body.style.display = "none";

  window.addEventListener('message', event => nuiEventNext(event.data));
  window.addEventListener('keydown', (event) => {
    if (event.code === 'Escape') {
      hideNui();
      viewType.value = '';
    }
  });

  const style = document.createElement('style');
  style.appendChild(document.createTextNode(`\
    @font-face {
      font-family: 'Satoshi';
      src: url('${isDev ? '' : '../'}Satoshi-Variable.woff2') format('woff2');
      font-weight: 300 900;
      font-display: block;
      font-style: normal;
    }
  `));
  document.head.appendChild(style);

  router.push('/');

  sendAvailabilityToClient();
  loadLocalization();
});

onNuiEvent<MenuObjectsResponse>(NuiEventType.SendMenuObjects).subscribe(event => {
  const menus: Menu[] = [];
  for (const menu of event.data) {
    const menuItems: MenuItem[] = [];
    for (const item of menu.Items) {
      let _item: any;
      switch (item.Type) {
        case MenuItemType.Button: {
          _item = new MenuItemButton(item.Title, item.Description, item.Hashcode);
          break;
        }
        case MenuItemType.List: {
          _item = new MenuItemList(item.Title, item.Description, item.Hashcode);
          _item.setItems = item.Items;
          break;
        }
        case MenuItemType.Toggle: {
          _item = new MenuItemToggle(item.Title, item.Description, item.Hashcode);
          break;
        }
      }
      menuItems.push(_item);
    }
    menus.push(new Menu(menu.Id, menu.Title, menu.UseLocalization, menuItems));
  }
  store.commit('setMenus', menus);
});

onNuiEvent<RadialMenuObjectsResponse>(NuiEventType.SendRadialMenuObjects).subscribe(event => {
  store.commit('setRadialMenus', event.data);
});

onNuiEvent<LocalizationChangeResponse>(NuiEventType.LocalizationChange).subscribe(event => {
  store.commit('setLocalization', event.data);
});

// TODO: Move RadialMenu event to its component and completely remove the emitter package and useEmitter hook
onNuiEvent<ViewEventResponse>(NuiEventType.Mdt, NuiEventType.MenuView, NuiEventType.RadialMenu).subscribe(event => {
  viewType.value = event.type;
  currentMenuId.value = event.menuId ?? '#';
  if (currentMenuId.value !== '#') {
    emitter.emit('showRadialMenu', currentMenuId.value);
  }
  showNui();
});

onNuiEvent<IBaseNuiEvent>(NuiEventType.Close).subscribe(() => {
  document.body.style.display = 'none';
  viewType.value = '';
});

function loadLocalization() {
  post<ILocalization>(NuiEvents.GetLocalization).subscribe(localization => {
    store.commit('setLocalization', localization);
  });
}

function sendAvailabilityToClient() {
  post(NuiEventType.IsNuiReady).subscribe();
}
</script>

<template>
  <div class="main-container">
    <Main v-if="viewType === NuiEventType.Mdt" />
    <template v-if="viewType === NuiEventType.MenuView" v-for="item in (store.state.global.menus as Menu[])">
      <MenuView :menu="item" v-if="currentMenuId === item.id" />
    </template>
    <TimerBar />
  </div>
  <RadialMenu v-if="viewType === NuiEventType.RadialMenu" :menuId="currentMenuId" />
</template>

<style lang="sass">
\:root
  --clr-red: #FF3E45
  --clr-blue: #178EE9

  --clr-border: #686464
  --clr-border-secondary: transparent
  --clr-gray: #686464
  --clr-overlay: #1B1A1A
  --clr-bg: #1B1A1A
  --clr-bg-secondary: rgba(104,100,100,.1)
  --clr-bg-5: rgba(104,100,100,.05)
  --clr-bg-20: rgba(104,100,100,.2)
  --clr-bg-40: rgba(104,100,100,.4)
  --clr-bg-80: rgba(104,100,100,.8)
  --clr-text: rgba(255,255,255,1)
  --clr-text-primary: rgba(255,255,255,.9)
  --clr-text-secondary: rgba(255,255,255,.7)

  --radius-sm: 5px
  --radius: 10px
  --radius-lg: 15px

  --transition: all 0.15s cubic-bezier(0.33, 1, 0.68, 1)

  --layer-0: 0
  --layer-10: 10
  --layer-100: 100

  --page-header-height: 115px

*
  margin: 0
  outline: none
  box-sizing: border-box
  user-select: none
  letter-spacing: 0.2px
  -webkit-user-drag: none
  -webkit-font-smoothing: subpixel-antialiased
  font-family: 'Satoshi', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif
  transition: var(--transition)
  &::before, &::after
    transition: var(--transition)

#app
  display: flex
  flex-direction: column
  align-items: center
  justify-content: center
  width: 100vw
  height: 100vh

  .main-container
    position: relative
    z-index: 10
    max-width: 1900px
    width: 100%
    height: 100%
    @media (max-width: 1920px)
      width: 95vw

</style>
