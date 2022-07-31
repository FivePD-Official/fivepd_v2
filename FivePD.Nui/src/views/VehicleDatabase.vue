<script setup lang="ts">
import type { InputField } from '@interfaces';
import type { FormFields } from "@utils";

const onSearchSubmit = (fields: FormFields) => {
  console.log(fields)
}

const fields: InputField[] = [
  {
    type: 'text',
    name: 'license_plate',
    label: 'License plate',
    placeholder: '12ABC34',
    required: false,
  },
  {
    type: 'text',
    name: 'owner_lastname',
    label: 'Owner\'s lastname',
    placeholder: 'John',
    required: false,
  },
  {
    type: 'text',
    name: 'owner_firstname',
    label: 'Owner\'s firstname',
    placeholder: 'Doe',
    required: false,
  }
];
</script>

<template>
  <PageHeader title="Vehicle database" />
  <div class="content">
    <SearchPanel
      :fields="fields"
      title="Search"
      subtitle="All fields are optional. If empty, every vehicle will be displayed."
      button-text="Search"
      @on-submit="onSearchSubmit"
    />
  </div>
</template>

<style scoped lang="sass">
.content
  display: flex
  flex-direction: row
  flex-wrap: wrap

</style>
