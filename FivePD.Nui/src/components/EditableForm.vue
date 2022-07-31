<script setup lang="ts">
import { formatFormEvent, FormFields } from "@utils";
import type { InputField } from "@interfaces";
import { computed, onMounted, ref, watch } from "vue";

const props = defineProps<{
  title: string;
  fields: InputField[][];
  submitButtonLabel: string;
  onSubmit: (fields: FormFields) => void;
}>();

interface InputFieldWithError extends InputField {
  hasError?: boolean;
  value?: string | number;
}

const internalFields = ref<InputFieldWithError[][]>(props.fields as InputFieldWithError[][]);
const modified = computed(() => {
  const flattenedInternal = internalFields.value.flat();
  for (const item of flattenedInternal) {
    if (item.value !== item.defaultValue) {
      return true;
    }
  }
  return false;
});

onMounted(() => {
   setDefaultFieldValues();
});

watch(() => props.fields, () => {
  setDefaultFieldValues();
});

function setDefaultFieldValues() {
  const modifiedInternalFields: InputFieldWithError[][] = [];
  for (const col of props.fields) {
    const arr = [];
    for (const item of col) {
      (item as InputFieldWithError).value = item.defaultValue;
      arr.push(item);
    }
    modifiedInternalFields.push(arr);
  }
  internalFields.value = modifiedInternalFields;
}

function onInputValueChange(field: InputField, value: string | number) {
  const modifiedInternalFields: InputFieldWithError[][] = [];
  for (const col of internalFields.value) {
    const arr = [];
    for (const item of col) {
      if (item.name === field.name) {
        item.value = value;
      }
      arr.push(item);
    }
    modifiedInternalFields.push(arr);
  }
  internalFields.value = modifiedInternalFields;
}

function internalOnSubmit(event: any) {
  const formattedFields = formatFormEvent(event);
  if (!formattedFields) {
    return;
  }

  const modifiedInternalFields: InputFieldWithError[][] = [];
  for (const col of internalFields.value) {
    const arr = [];
    for (const item of col) {
      arr.push(item);
    }
    modifiedInternalFields.push(arr);
  }
  for (const [key, value] of Object.entries(formattedFields)) {
    let field: InputFieldWithError | undefined;
    for (const col of internalFields.value) {
      for (const item of col) {
        if (item.name === key) {
          field = item;
          break;
        }
      }
    }
    if (!field) {
      continue;
    }

    const isValueFalsy = field.type === 'number' ? isNaN(value) : !value;
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

  let hasError = false;
  for (const col of internalFields.value) {
    for (const item of col) {
      if (item.hasError) {
        hasError = true;
        break;
      }
    }
  }

  if (!hasError) {
    props.onSubmit(formattedFields);
  }
}
</script>

<template>
  <form @submit.prevent="internalOnSubmit">
    <p class="common-title">{{ title }}</p>
    <div class="form-columns" :style="{ grid: `1fr / repeat(${fields.length}, 1fr)` }">
      <div class="form-column" v-for="col in internalFields">
        <InputField v-for="field in col" :field="field" :hasError="field.hasError" :onValueChange="value => onInputValueChange(field, value)" />
      </div>
    </div>
    <div class="button-wrapper">
      <Button
          :disabled="!modified"
          :label="submitButtonLabel"
          type="submit"
          theme="primary"
      />
    </div>
  </form>
</template>

<style scoped lang="sass">
form
  .form-columns
    margin-top: 15px
    display: grid
    gap: 30px
    
  .button-wrapper
    width: 100%
    display: flex
    flex-direction: row
    justify-content: flex-end

    button
      max-width: 120px
</style>