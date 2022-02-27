<script setup lang="ts">
import { watch, ref } from 'vue';
import { useStore } from 'vuex';

const store = useStore();
// const dark = ref(false);
// const s = watch(dark, () => {
//   store.dispatch('toggleDarkTheme');
// });
</script>

<template>
  <PageHeader title="Settings" />
  <!-- <label for="dark">toggle dark theme</label>
  <input type="checkbox" id="dark" v-model="dark" true-value="true" false-value="false"> -->
</template>

<style scoped lang="sass">

</style>
