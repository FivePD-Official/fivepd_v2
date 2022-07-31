<script setup lang="ts">
import { DateFormat } from "@enums";
import type { InputField } from "@interfaces";
import { createBirthdateInputList } from "@utils";
import { ref, watch } from "vue";

const props = defineProps<{
  field: InputField,
  hasError?: boolean,
  onValueChange?: (value: string | number) => void,
}>();

const dirty = ref(false);
const hasError = ref(false);
const value = ref(props.field.defaultValue);

watch(() => props.hasError, (newValue) => {
  hasError.value = newValue!;
});

function onInput(event: any) {
  if (!dirty.value) {
    dirty.value = true;
  }

  const value = event.target.value;

  const isValueFalsy = props.field.type === 'number' ? isNaN(value) : !value;
  if (props.field.required) {
    if (props.field.validator) {
      hasError.value = props.field.validator(value);
    } else {
      hasError.value = isValueFalsy;
    }
  } else if (value && props.field.validator) {
    hasError.value = props.field.validator(value);
  } else {
    hasError.value = false;
  }
  
  if (!hasError.value && props.onValueChange) {
    props.onValueChange(value);
  }
}
</script>

<template>
  <div class="input-box">
    <label
      v-if="field.label"
      :class="{
        'error': hasError,
      }"
    >
      {{ field.label }}
    </label>
    <input
      v-if="field.type !== 'date'"
      v-model="value"
      :type="field.type"
      :name="field.name"
      :placeholder="field.placeholder"
      @input="event => onInput(event)"
    >
    <div v-else class="inputs">
      <input
        v-for="input in createBirthdateInputList(DateFormat.MDY)"
        :key="input.name"
        type="number"
        min="0"
        :name="input.name"
        :placeholder="input.placeholder"
      >
    </div>
    <span v-if="field.helpLabel" class="help-label" v-html="field.helpLabel" />
  </div>
</template>

<style scoped lang="sass">
.input-box
  width: 100%
  margin-bottom: 20px

  label
    display: block
    font-size: 16px
    color: var(--clr-text-secondary)
    margin-bottom: 5px
    &.error
      color: var(--clr-red)

  input
    height: 40px
    width: 100%
    border: 1px solid var(--clr-bg-40)
    background: transparent
    border-radius: var(--radius-sm)
    padding: 0 15px
    font-size: 16px
    color: var(--clr-text-secondary)
    &::placeholder
      color: var(--clr-text-secondary)
      opacity: .4
    &:focus
      border-color: var(--clr-gray)

  .help-label
    display: block
    font-size: 16px
    color: var(--clr-text-tertiary)
    margin-top: 5px

  .inputs
    display: grid
    grid: 1fr / repeat(3, 1fr)
    gap: 5px

    input
      &::-webkit-outer-spin-button, &::-webkit-inner-spin-button
        appearance: none
        -webkit-appearance: none
</style>
