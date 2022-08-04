<script setup lang="ts">
import type { InputField } from '@interfaces';
import { FormFields } from '@utils';

const emit = defineEmits(['onSubmit']);

defineProps<{
  fields: InputField[],
  title: string,
  subtitle?: string,
  buttonText: string
}>();

function onSearchSubmit(fields: FormFields) {
  emit('onSubmit', fields);
}
</script>

<template>
  <Form
    submit-button-label="Search"
    :fields="fields"
    :on-submit="onSearchSubmit"
    :title="title"
    :subtitle="subtitle"
  />
</template>

<style scoped lang="sass">
form
  background: var(--clr-bg-secondary)
  border-radius: var(--radius)
  max-width: 300px
  height: max-content
  position: sticky
  top: var(--page-header-height)
  border: 1px solid var(--clr-border-secondary)
  padding: 20px
</style>
