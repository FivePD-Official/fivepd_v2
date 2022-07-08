<script setup lang="ts">
import { DateFormat } from "@enums";
import type { InputField } from "@interfaces";
import { createBirthdateInputList } from "@utils";

const props = defineProps<{
  field: InputField
}>();
</script>

<template>
  <div class="input-box">
    <label :for="field.name">{{ field.label }}</label>
    <input
      :type="field.type"
      :name="field.name"
      :id="field.name"
      :placeholder="field.placeholder"
      v-if="field.type !== 'date'"
    />
    <div class="inputs" v-else>
      <input
        type="number"
        min="0"
        :name="input.name"
        id="birthdate"
        :placeholder="input.placeholder"
        v-for="input in createBirthdateInputList(DateFormat.MDY)"
      />
    </div>
    <span class="help-label" v-if="field.helpLabel">{{ field.helpLabel }}</span>
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
