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
      :type="field.type"
      :id="field.name"
      :name="field.name"
      :placeholder="field.placeholder"
      v-if="field.type !== 'date' && field.type !== 'checkboxGroup'"
      @input="event => onInput(event)"
      v-model="value"
    />
    <div class="inputs" v-else-if="field.type === 'date'">
      <input
        type="number"
        min="0"
        :id="input.name"
        :name="input.name"
        :placeholder="input.placeholder"
        v-for="input in createBirthdateInputList(DateFormat.MDY)"
      />
    </div>
    <div class="checkbox-group" v-else>
      <div class="checkbox" v-for="checkbox in field.checkboxes">
        <div class="checkbox-inner">
          <input type="checkbox" :value="checkbox.id" :name="field.name + '-' + checkbox.id" :id="field.name + '-' + checkbox.id" :checked="checkbox.defaultValue ?? false">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <label :for="field.name + '-' + checkbox.id">{{ checkbox.label }}</label>
      </div>
    </div>
    <span class="help-label" v-if="field.helpLabel" v-html="field.helpLabel"></span>
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
        
  .checkbox-group
    margin-top: 10px

    .checkbox
      display: flex
      flex-direction: row
      align-items: center
      margin-bottom: 10px
      
      .checkbox-inner
        position: relative
        width: 24px
        height: 24px

        svg
          position: absolute
          top: 50%
          left: 50%
          transform: translate(-50%,-50%)
          height: 22px
          opacity: 0
          color: white
          z-index: var(--layer-10)
          
        input
          position: absolute
          top: 0
          left: 0
          width: 100%
          height: 100%
          border: 2px solid var(--clr-bg-80)
          border-radius: var(--radius-sm)
          z-index: var(--layer-0)
          appearance: none
          -webkit-appearance: none
          background: transparent
          padding: 0
          &:checked
            border-color: var(--clr-text-secondary)
            & ~ svg
              opacity: 1
        
      label
        font-size: 16px
        color: var(--clr-text-secondary)
        cursor: pointer
        margin-left: 10px
        margin-bottom: 0
</style>
