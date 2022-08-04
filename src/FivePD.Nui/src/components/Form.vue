<script setup lang="ts">
import type { InputField } from '@interfaces';
import { formatFormEvent, FormFields } from '@utils';
import { ref, useSlots } from 'vue';

const slots = useSlots();
const props = defineProps<{
  fields: InputField[];
  submitButtonLabel?: string;
  onSubmit: (fields: FormFields) => void;
  title?: string;
  subtitle?: string;
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

  const modifiedInternalFields = [...internalFields.value];
  for (const [key, value] of Object.entries(formattedFields)) {
    const field = modifiedInternalFields.find(field => field.name === key)!;

    const isValueFalsy = field.type === 'text' ? !value : false;
    if (field.required) {
      if (field.validator) {
        field.hasError = field.validator(value);
      } else {
        field.hasError = isValueFalsy;
      }
    } else if (value && field.validator) {
      field.hasError = field.validator(value);
    } else {
      field.hasError = false;
    }
  }

  internalFields.value = modifiedInternalFields;

  if (!modifiedInternalFields.some(field => field.hasError)) {
    props.onSubmit(formattedFields);
  }
}
</script>

<template>
  <form @submit.prevent="internalOnSubmit">
    <p v-if="title" class="form-title">
      {{ title }}
    </p>
    <p v-if="subtitle" class="form-subtitle">
      {{ subtitle }}
    </p>
    <InputField v-for="(field, fieldIndex) in internalFields" :key="fieldIndex" :field="field" :has-error="field.hasError" />
    <Button v-if="!slots.default && submitButtonLabel" :label="submitButtonLabel" theme="primary" type="submit" />
    <slot />
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