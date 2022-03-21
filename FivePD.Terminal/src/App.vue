<script setup lang="ts">
import { hideNui, showNui, NuiEventType, Post, NuiEvents, Menu, MenuItem, MenuItemButton, MenuItemList, MenuItemToggle } from '@utils';
import { useRouter } from 'vue-router';
import { type Ref, ref } from 'vue';
import { useEmitter } from '@hooks';
import { MenuItemType } from '@enums';
import { useStore } from 'vuex';

const store = useStore();
const emitter = useEmitter();
const router = useRouter();
const viewType: Ref<string> = ref<string>(NuiEventType.Mdt);
const subtype: Ref<string> = ref<string>('#');

window.onload = function () {

  const isDev = import.meta.env.MODE === 'development';

  document.head.innerHTML = document.head.innerHTML + '<base href=' + (isDev ? '' : '/Libraries/FivePD.Terminal/') + '/>';
  document.body.style.display = "none";

  window.addEventListener('message', (event) => {
    if (!event.data.hasOwnProperty('type')) return;

    // Type should always hold the same values as in FivePD.Gamemode.Client.Services.NuiService.NuiEventType
    const type: string = event.data.type;

    if (type === NuiEventType.LocalizationChange) {
      store.commit('setLocalization', JSON.parse(event.data.data));
      return;
    }

    if (![NuiEventType.MenuDown, NuiEventType.MenuUp, NuiEventType.MenuLeft, NuiEventType.MenuRight, NuiEventType.MenuSelect].includes(type)) {
      subtype.value = event.data.hasOwnProperty('subtype') ? event.data.subtype : '#';
    }

    if (isDev) console.table(event.data);

    if (type === NuiEventType.Close) {
      document.body.style.display = "none";
      return;
    }

    if (subtype.value !== '#') {
      emitter.emit('showRadialMenu', subtype.value);
    }

    console.log(type, subtype.value)

    if ([NuiEventType.Mdt, NuiEventType.MenuView, NuiEventType.RadialMenu].includes(type)) {
      viewType.value = type;
      showNui();
      return;
    }

    emitter.emit(type);
  });

  window.addEventListener('keydown', (event) => {
    if (event.code === 'Escape') hideNui();
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

  loadLocalization();
  loadMenus();
  loadRadialMenus();
}

const loadLocalization = async () => {
  const localization: any = await Post(NuiEvents.GetLocalization).catch(() => {});
  store.commit('setLocalization', JSON.parse(localization));
  console.log('Localization', JSON.parse(localization));
}

const loadMenus = async () => {
  const res: any = await Post(NuiEvents.GetMenus).catch(() => {});
  const menusJson: any = JSON.parse(res);
  const menus: Menu[] = [];
  for (const menu of menusJson) {
    const menuItems: MenuItem[] = [];
    for (const item of menu.Items) {
      let _item: any;
      switch (item.Type) {
        case MenuItemType.Button: _item = new MenuItemButton(item.Title, item.Description, item.Hashcode); break;
        case MenuItemType.List: {
          _item = new MenuItemList(item.Title, item.Description, item.Hashcode);
          _item.setItems = JSON.parse(item.Items);
          break;
        }
        case MenuItemType.Toggle: _item = new MenuItemToggle(item.Title, item.Description, item.Hashcode); break;
      }
      menuItems.push(_item);
    }
    menus.push(new Menu(menu.Hashcode, menu.Title, menu.UseLocalization, menuItems));
  }
  console.log(menus);
  store.commit('setMenus', menus);
}

const loadRadialMenus = async () => {
  const res: any = await Post(NuiEvents.GetRadialMenus).catch(() => {});
  store.commit('setRadialMenus', JSON.parse(res));
}
</script>

<template>
  <Main v-if="viewType === NuiEventType.Mdt" />
  <template v-if="viewType === NuiEventType.MenuView" v-for="item in store.state.global.menus">
    <MenuView :menu="(item as Menu)" v-if="subtype === (item as Menu).id" />
  </template>
  <RadialMenu v-if="viewType === NuiEventType.RadialMenu" :menuHashcode="subtype" />
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

</style>
