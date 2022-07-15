<script setup lang="ts">
import { ClientEvents } from '@enums';
import { ActionResponse } from '@interfaces';
import { useState } from '@store';
import { FormFields, post } from '@utils';
import { useRouter } from 'vue-router';

const state = useState();
const router = useRouter();

function onSubmit(fields: FormFields) {
  post<ActionResponse>(ClientEvents.CreateDepartment, fields).subscribe((res) => {
    // TODO: success / error notification
    if (res.Status) {
      router.push('/');
    } else {

    }
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
          validator: (value: string) => !new RegExp('^[a-zA-Z]+$').test(value),
        },
        {
          name: 'CallsignFormat',
          label: state.global.localization.Nui.Labels.CallsignFormat,
          type: 'text',
          required: true,
          helpLabel: `
            ${state.global.localization.Nui.Labels.CallsignFormatHelpLabel} ${state.global.localization.Nui.Labels.AvailableCharacters}: <br/>
            <table>
              <tr>
                <td><b>L</b><td>
                <td>(${state.global.localization.Nui.Labels.Letter})<td>
              </tr>
              <tr>
                <td><b>N</b><td>
                <td>(${state.global.localization.Nui.Labels.Number})<td>
              </tr>
              <tr>
                <td><b>-</b><td>
                <td>(${state.global.localization.Nui.Labels.Dash})<td>
              </tr>
            </table>
          `,
          validator: (value: string) => !new RegExp('^[a-zA-Z0-9\-*]{1,10}$').test(value),
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