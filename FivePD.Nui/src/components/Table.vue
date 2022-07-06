<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  title: string,
  columns: Record<string, string>,
  rows: any[],
}>();

const formattedRows = computed(() => {
  return props.rows.map(item => {
    const row: any = [];
    for (const key of Object.keys(props.columns)) {
      row.push(item[key]);
    }
    return row;
  })
})
</script>

<template>
  <div class="table-container">
    <p class="common-title">{{ title }}</p>
    <div v-if="rows?.length" class="common-bg table">
      <div class="table-row table-header">
        <div class="table-cell" v-for="column in Object.values(columns)">
          <p>{{ column }}</p>
        </div>
      </div>
      <div class="table-row" v-for="row in formattedRows">
        <div class="table-cell" v-for="item in row">
          <p>{{ item }}</p>
        </div>
      </div>
    </div>
    <div v-else>
      <p class="no-data">No data to display.</p>
    </div>
  </div>
</template>

<style scoped lang="sass">
.table-container
  margin-bottom: 40px

  .no-data
    font-size: 20px
    color: var(--clr-text-secondary)

  .table

    .table-row
      display: grid
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr))
      grid-gap: 10px
      margin-bottom: 10px
      &:last-of-type
        margin-bottom: 0

      .table-cell
        p
          font-size: 18px
          color: var(--clr-text-secondary)

      &.table-header
        .table-cell
          p
            font-size: 20px
            font-weight: 700
            color: var(--clr-text)

</style>

