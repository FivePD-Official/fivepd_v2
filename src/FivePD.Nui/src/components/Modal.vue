<script setup lang="ts">
import type { FormFields } from '@utils';
import { type Modal, modalService } from '@services';

defineProps<{
  modal: Modal;
}>();

function submit(fields?: FormFields) {
  if (fields) {
    modalService.submit(fields);
  } else {
    modalService.submit();
  }
}
</script>

<template>
  <div class="modal-backdrop">
    <div class="modal">
      <p class="title">{{ modal.title }}</p>
      <p v-if="modal.description" class="description" v-html="modal.description" />
      <Form v-if="modal.inputs?.length" :fields="modal.inputs" :on-submit="submit">
        <div class="buttons-wrapper">
          <div class="buttons">
            <Button
              :theme="modal.buttons.cancel.theme"
              :label="modal.buttons.cancel.label"
              :on-click="() => modalService.close()"
            />
            <Button
              type="submit"
              :theme="modal.buttons.submit.theme"
              :label="modal.buttons.submit.label"
            />
          </div>
        </div>
      </Form>
      <div v-else class="buttons-wrapper">
        <div class="buttons">
          <Button
            :theme="modal.buttons.cancel.theme"
            :label="modal.buttons.cancel.label"
            :on-click="() => modalService.close()"
          />
          <Button
            type="submit"
            :theme="modal.buttons.submit.theme"
            :label="modal.buttons.submit.label"
            :on-click="() => submit()"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="sass">
.modal-backdrop
  position: absolute
  top: 0
  left: 0
  width: 100%
  height: 100%
  background: rgba(0,0,0,.5)
  z-index: var(--layer-100)

  .modal
    position: absolute
    top: 50%
    left: 50%
    transform: translate(-50%,-50%)
    padding: 20px
    border-radius: var(--radius)
    background: var(--clr-bg-secondary)
    width: 500px
    
    .title
      font-size: 22px
      font-weight: 700
      color: var(--clr-text-primary)

    .description
      font-size: 18px
      color: var(--clr-text-secondary)
      margin: 10px 0 15px 0

    form
      margin-top: 20px
      max-width: none
      
    .buttons-wrapper
      width: 100%
      display: flex
      flex-direction: row
      align-items: center
      justify-content: flex-end
      margin-top: 20px

      .buttons
        width: 50%
        display: flex
        flex-direction: row
        align-items: center
        justify-content: flex-end
        gap: 10px
</style>
