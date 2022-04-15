<script setup lang="ts">
import { hideNui, showNui, Post, Menu, MenuItem, MenuItemButton, MenuItemList, MenuItemToggle } from '@utils';
import { NuiEventType, NuiEvents } from '@enums';
import { useRouter } from 'vue-router';
import { ref } from 'vue';
import { useEmitter } from '@hooks';
import { MenuItemType } from '@enums';
import { useStore } from 'vuex';

const store = useStore();
const emitter = useEmitter();
const router = useRouter();
const viewType = ref<string>('');
const currentMenuId = ref<string>('#');

window.onload = function () {

  const isDev = import.meta.env.MODE === 'development';

  document.head.innerHTML = document.head.innerHTML + '<base href=' + (isDev ? '' : '/Libraries/FivePD.Terminal/') + '/>';
  document.body.style.display = "none";

  window.addEventListener('message', event => handleEvents(event, isDev));
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
}

const handleEvents = (event: any, isDev: boolean) => {
  if (!event.data.hasOwnProperty('type')) return;

  const type: NuiEventType = event.data.type;

  switch (type) {
    case NuiEventType.SendMenuObjects: {
      loadMenus(event.data.data);
      break;
    }
    case NuiEventType.SendRadialMenuObjects: {
      loadRadialMenus(event.data.data);
      break;
    }
    case NuiEventType.LocalizationChange: {
      store.commit('setLocalization', JSON.parse(event.data.data));
      break;
    }
    case NuiEventType.MenuUp:
    case NuiEventType.MenuDown:
    case NuiEventType.MenuLeft:
    case NuiEventType.MenuRight:
    case NuiEventType.MenuSelect: {
      emitter.emit(type);
      break;
    }
    case NuiEventType.Mdt:
    case NuiEventType.MenuView:
    case NuiEventType.RadialMenu: {
      viewType.value = type;
      currentMenuId.value = event.data.hasOwnProperty('menuId') ? event.data.menuId : '#';
      if (currentMenuId.value !== '#') {
        emitter.emit('showRadialMenu', currentMenuId.value);
      }
      showNui();
      console.log({type})
      break;
    }
    case NuiEventType.Close: {
      document.body.style.display = "none";
      viewType.value = '';
      break;
    }
  }
}

const loadLocalization = async () => {
  const localization: any = await Post(NuiEvents.GetLocalization).catch(() => {});
  store.commit('setLocalization', JSON.parse(localization));
}

const loadMenus = async (data: string) => {
  const menusJson: any = JSON.parse(data);
  const menus: Menu[] = [];
  for (const menu of menusJson) {
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
          _item.setItems = JSON.parse(item.Items);
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
}

const loadRadialMenus = async (data: string) => {
  store.commit('setRadialMenus', JSON.parse(data));
}

const sendAvailabilityToClient = () => {
  Post(NuiEventType.IsNuiReady).catch(() => {})
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
