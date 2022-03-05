<script setup lang="ts">
import { hideNui, showNui, NuiEventType, Post, NuiEvents } from '@utils';
import { useRouter } from 'vue-router';
import { type Ref, ref } from 'vue';
import { useEmitter } from '@hooks';
import { store } from '@store';

const emitter = useEmitter();
const router = useRouter();
const viewType: Ref<string> = ref<string>(NuiEventType.Mdt);

window.onload = function () {

  const isDev = import.meta.env.MODE === 'development';

  document.head.innerHTML = document.head.innerHTML + '<base href=' + (isDev ? '' : '/Libraries/FivePD.Terminal/') + '/>';
  document.body.style.display = "none";

  window.addEventListener('message', (event) => {
    if (!event.data.hasOwnProperty('type')) return;

    // Type should always hold the same values as in FivePD.Gamemode.Client.Services.NuiService.NuiEventType
    const type: string = event.data.type;

    if (type === NuiEventType.Close) {
      document.body.style.display = "none";
      return;
    }

    if ([NuiEventType.Mdt, NuiEventType.DutyMenu].includes(type)) {
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

  loadAvailableLocales();
  loadLocalization();
}

const loadAvailableLocales = async () => {
  const locales: any = await Post(NuiEvents.GetAvailableLocalesFromNui).catch(() => {});
  console.log(JSON.parse(locales));
  store.commit('setAvailableLocales', JSON.parse(locales));
}

const loadLocalization = async () => {
  const localization: any = await Post(NuiEvents.GetLocalization).catch(() => {});
  store.commit('setLocalization', JSON.parse(localization));
}
</script>

<template>
  <Main v-if="viewType === NuiEventType.Mdt" />
  <DutyMenu v-else-if="viewType === NuiEventType.DutyMenu" />
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
