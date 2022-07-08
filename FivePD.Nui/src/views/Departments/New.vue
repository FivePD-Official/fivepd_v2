<script setup lang="ts">
import { ClientEvents } from '@enums';
import { useState } from '@store';
import { FormFields, post } from '@utils';
import { useRouter } from 'vue-router';

const state = useState();
const router = useRouter();

function onSubmit(fields: FormFields) {
  post(ClientEvents.CreateDepartment, fields).subscribe((res) => {
    console.log(res)
    router.push('/');
    // TODO: success / error notification
  });
}
</script>

<template>
  <PageHeader :title="state.global.localization.Nui.Labels.NewDepartment" :centerTitle="true" />

  <div class="form-wrapper">
    <Form
      :submitButtonLabel="state.global.localization.Nui.Actions.Save"
      :fields="[
        {
          name: 'Name',
          label: state.global.localization.Nui.Labels.Name,
          type: 'text',
          required: true,
        },
        {
          name: 'Acronym',
          label: state.global.localization.Nui.Labels.Acronym,
          type: 'text',
          required: false,
          helpLabel: state.global.localization.Nui.Labels.AcronymHelpLabel,
        },
        {
          name: 'CallsignFormat',
          label: state.global.localization.Nui.Labels.CallsignFormat,
          type: 'text',
          required: true,
        },
        {
          name: 'MaxUsers',
          label: state.global.localization.Nui.Labels.MaxNumberOfMembers,
          type: 'number',
          required: true,
          defaultValue: 0,
          helpLabel: state.global.localization.Nui.Labels.MaxNumberOfMembersHelpLabel,
        }
      ]"
      :onSubmit="onSubmit"
    />
  </div>
</template>

<style scoped lang="sass">
.form-wrapper
  display: flex
  flex-direction: column
  align-items: center
</style>