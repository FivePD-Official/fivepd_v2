<script setup lang="ts">
import {
  hideNui,
  Menu,
  MenuButtonItem,
  MenuCheckboxItem,
  MenuItem,
  MenuListItem,
  nuiEventNext,
  onNuiEvent,
  post,
  showNui,
} from '@utils';
import { ClientEvents, MenuItemType, NuiEventType } from '@enums';
import { useRoute, useRouter } from 'vue-router';
import { onMounted, ref, watch } from 'vue';
import { useStore } from 'vuex';
import type {
  BaseNuiEvent,
  ILocalization,
  LocalizationChangeEvent,
  MenuObjectsEvent,
  MenuShowEvent,
  RadialMenuObjectsEvent,
  SoundEvent,
  SoundSpeakEvent,
  UpdateMenuItemEvent,
} from '@interfaces';
import { State } from '@store';
import { IUser } from "@interfaces";

const store = useStore<State>();
const router = useRouter();
const viewType = ref<string>('');
const currentMenuId = ref<string>('#');
const audioMap = new Map<string, any>();

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

onNuiEvent<MenuObjectsEvent>(NuiEventType.SendMenuObjects).subscribe(event => {
  const menus: Menu[] = [];
  for (const menu of event.data) {
    const menuItems: MenuItem[] = [];
    for (const item of menu.Items) {
      let _item: any;
      switch (item.Type) {
        case MenuItemType.Button: {
          _item = new MenuButtonItem(item.Title, item.Description, item.Hashcode);
          break;
        }
        case MenuItemType.List: {
          _item = new MenuListItem(item.Title, item.Description, item.Hashcode);
          _item.items = item.Items;
          break;
        }
        case MenuItemType.Toggle: {
          _item = new MenuCheckboxItem(item.Title, item.Description, item.Hashcode);
          break;
        }
      }
      menuItems.push(_item);
    }
    menus.push(new Menu(menu.Id, menu.Title, menu.UseLocalization, menuItems));
  }
  store.commit('setMenus', menus);
});

onNuiEvent<RadialMenuObjectsEvent>(NuiEventType.SendRadialMenuObjects).subscribe(event => {
  store.commit('setRadialMenus', event.data);
});

onNuiEvent<LocalizationChangeEvent>(NuiEventType.LocalizationChange).subscribe(event => {
  store.commit('setLocalization', event.data);
});

onNuiEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(event => {
  viewType.value = event.type;
  showNui();
  post<IUser>(ClientEvents.GetPlayerDetails).subscribe((res) => {
    store.commit('setUserDetails', res);
  });
});

onNuiEvent<MenuShowEvent>(NuiEventType.MenuView, NuiEventType.RadialMenu).subscribe(event => {
  viewType.value = event.type;
  currentMenuId.value = event.menuId;
  showNui();
});

onNuiEvent<BaseNuiEvent>(NuiEventType.Close).subscribe(() => {
  document.body.style.display = 'none';
  viewType.value = '';
});

onNuiEvent<UpdateMenuItemEvent>(NuiEventType.UpdateMenuItem).subscribe(event => {
  for (const menu of (store.state.global.menus as Menu[])) {
    for (const item of menu.items) {
      if (item.hashcode === event.hashcode) {
        if (Reflect.has(item, event.fieldname)) {
          Reflect.set(item, event.fieldname, event.value);
        }
        break;
      }
    }
  }
});

onNuiEvent<SoundEvent>(NuiEventType.PlaySound).subscribe(event => {
  if (audioMap.has(event.sound)) {
    const audio = audioMap.get(event.sound);
    if (audio.paused) {
      audio.play();
    }
    return;
  }

  const audio = new Audio(`./sounds/${event.sound}`);
  audioMap.set(event.sound, audio);
  audio.play();
});

onNuiEvent<SoundSpeakEvent>(NuiEventType.PlayText).subscribe(event => {
  if (audioMap.has(event.textToPlay)) {
    return;
  }

  const audio = new SpeechSynthesisUtterance(event.textToPlay);
  audioMap.set(event.textToPlay, audio);
  speechSynthesis.speak(audio);
  audio.onend = () => {
    audioMap.delete(event.textToPlay);
  };
});

function loadLocalization() {
  post<ILocalization>(ClientEvents.GetLocalizationFromNui).subscribe(localization => {
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
    <template v-if="viewType === NuiEventType.MenuView" v-for="item in store.state.global.menus">
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
  --clr-bg-secondary: #222121
  --clr-bg-5: #1F1E1E
  --clr-bg-20: #2B2929
  --clr-bg-40: #3A3838
  --clr-bg-80: #585555
  --clr-text: rgba(255,255,255,1)
  --clr-text-primary: rgba(255,255,255,.9)
  --clr-text-secondary: rgba(255,255,255,.7)
  --clr-text-tertiary: rgba(255,255,255,.5)

  --radius-sm: 5px
  --radius: 10px
  --radius-lg: 15px

  --transition: all 0.15s cubic-bezier(0.33, 1, 0.68, 1)

  --layer-0: 0
  --layer-10: 10
  --layer-100: 100
  --layer-1000: 1000

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
    max-width: 85vw
    width: 100%
    height: 100%
    @media (max-width: 1920px)
      width: 95vw

.common-title
  font-size: 22px
  font-weight: 700
  color: var(--clr-text-primary)

.common-bg
  border-radius: var(--radius)
  background-color: var(--clr-bg-secondary)
  padding: 20px

</style>
