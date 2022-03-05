<script setup lang="ts">
import { MenuItemList, MenuItemToggle, NuiEvents, Post } from '@utils';
import { useT } from '@hooks';
import { useStore } from 'vuex';

const store = useStore();
const t = useT();

const onDutyToggle = (obj?: MenuItemToggle) => {
  if (!obj) return;
}
const getLoadout = async (obj?: MenuItemList) => {
  if (!obj) return;

  const res = await Post(NuiEvents.GetLoadout).catch(() => {});
  console.log(res);
}
const changeLanguage = async (obj?: MenuItemList) => {
  if (!obj) return;

  const res: any = await Post(NuiEvents.ChangeLocalization, {
    locale: obj.items[obj.currentIndex].Key
  }).catch(() => {});

  store.commit('setLocalization', JSON.parse(res));
}

const dutyToggle = new MenuItemToggle('On duty', 'Toggles your duty status.', onDutyToggle);

const loadoutItem = new MenuItemList('Get loadout', 'Loadout.', getLoadout);
loadoutItem.setItems = [{ Key: 0, Title: 'test' }, { Key: 1, Title: 'test 2'}];

const localizationItem = new MenuItemList('Change language', 'Language.', changeLanguage);
localizationItem.setItems = store.state.user.availableLocales;
</script>

<template>
  <Menu :title="t('DutyMenu.Title')" :items="[dutyToggle, loadoutItem, localizationItem]" />
</template>

<style lang="sass">
</style>
