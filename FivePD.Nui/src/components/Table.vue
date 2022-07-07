<script setup lang="ts">
import { useState } from '@store';
import { computed } from 'vue';

interface Column {
  replaceKeyFrom?: any[],
  linkToId?: string,
  label: string,
};

interface FormattedColumn {
  value: string,
  isLink: boolean,
}

interface FormattedRowItem {
  value: string,
  link?: string,
}

type FormattedRow = FormattedRowItem[];

const state = useState();
const props = defineProps<{
  title: string,
  columns: Record<string, string | Column>,
  rows: any[],
}>();

const formattedColumns = computed<FormattedColumn[]>(() => {
  return Object.values(props.columns).map(item => {
    const newValue: FormattedColumn = {
      value: typeof item === 'string' ? item : item.label,
      isLink: typeof item !== 'string' && !!item.linkToId
    }
    return newValue;
  });
});

const formattedRows = computed<FormattedRow[]>(() => {
  return props.rows.map(item => {
    const row: FormattedRow = [];

    for (const [key, value] of Object.entries(props.columns)) {
      let newValue = '';

      if (typeof value === 'string') {
        newValue = item[key];
      } else if (value.replaceKeyFrom) {
        newValue = value.replaceKeyFrom.find(sub => sub.Id === item[key])?.Name;
      } else {
        newValue = item[key];
      }

      const rowItem: FormattedRowItem = {
        value: newValue,
      };

      if (typeof value !== 'string' && value.linkToId) {
        rowItem.link = `${value.linkToId}/${item.Id}`
      }
      row.push(rowItem);
    }

    return row;
  })
});
</script>

<template>
  <div class="table-container">
    <p class="common-title">{{ title }}</p>
    <div v-if="rows?.length" class="common-bg table">
      <div class="table-row table-header">
        <div class="table-cell" v-for="column in formattedColumns">
          <p>{{ column.value }}</p>
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" v-if="column.isLink">
            <path stroke-linecap="round" stroke-linejoin="round" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
          </svg>
        </div>
      </div>
      <div class="table-row" v-for="row in formattedRows">
        <div class="table-cell" v-for="item in row">
          <router-link v-if="item.link" :to="item.link">{{ item.value }}</router-link>
          <p v-else>{{ item.value }}</p>
        </div>
      </div>
    </div>
    <div v-else>
      <p class="no-data">{{ state.global.localization.Nui.Labels.NoDataToDisplay }}</p>
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
        p, a
          font-size: 18px
          color: var(--clr-text-secondary)
          text-decoration: none

        a:hover
          color: var(--clr-text)

      &.table-header
        .table-cell
          display: flex
          flex-direction: row
          align-items: center

          p
            font-size: 20px
            font-weight: 700
            color: var(--clr-text)

          svg
            margin-left: 10px
            height: 20px
            color: var(--clr-text-secondary)

</style>

