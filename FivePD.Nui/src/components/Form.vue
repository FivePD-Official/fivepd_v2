<script setup lang="ts">
import type { InputField } from '@interfaces';
import { formatFormEvent, FormFields } from '@utils';
import { ref } from 'vue';

const props = defineProps<{
  fields: InputField[],
  submitButtonLabel: string,
  onSubmit: (fields: FormFields) => void,
  title?: string,
  subtitle?: string,
}>();

interface InputFieldWithError extends InputField {
  hasError?: boolean;
}

const internalFields = ref<InputFieldWithError[]>(props.fields as InputFieldWithError[]);

function internalOnSubmit(event: any) {
  const formattedFields = formatFormEvent(event);
  if (!formattedFields) {
    return;
  }

  for (const [key, value] of Object.entries(formattedFields)) {
    const field = internalFields.value.find(field => field.name === key);
    if (!field?.required) {
      continue;
    }

    field.hasError = field.type === 'number' ? isNaN(value) : !value;
  }

  if (!internalFields.value.some(field => field.hasError)) {
    props.onSubmit(formattedFields);
  }
}
</script>

<template>
  <form @submit.prevent="internalOnSubmit">
    <p class="form-title" v-if="title">{{ title }}</p>
    <p class="form-subtitle" v-if="subtitle">{{ subtitle }}</p>
    <InputField v-for="field in internalFields" :field="field" :hasError="field.hasError" />
    <Button :label="submitButtonLabel" theme="primary" type="submit" />
  </form>
</template>

<style scoped lang="sass">
form
  position: relative
  display: flex
  flex-direction: column
  align-items: flex-start
  max-width: 350px
  width: 100%
  justify-self: center

  .form-title
    font-size: 22px
    font-weight: 700
    color: var(--clr-text-primary)
    margin-bottom: 5px

  .form-subtitle
    font-size: 16px
    color: var(--clr-text-secondary)
    margin-bottom: 25px

  button
    width: 100%
</style>