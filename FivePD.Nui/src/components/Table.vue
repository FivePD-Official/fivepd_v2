<script setup lang="ts">
import { useState } from '@store';
import { computed, onMounted, onUnmounted, ref } from 'vue';
import type {
  FormattedRow,
  FormattedRowItem,
  PropButton,
  ColumnDef, FormFilter, Page, BaseRequest
} from '@interfaces';
import { debounceTime, Observable, Subject } from "rxjs";

const state = useState();
const props = defineProps<{
  title: string;
  buttons?: PropButton[];
  columns: ColumnDef<any>;
  defaultData?: any;
  dataRequest?: (request: BaseRequest) => Observable<Page<any>>;
  dataMapper?: (response: Page<any>) => Page<any>;
}>();

const data = ref<Page<any>>({
  Items: [],
  PageCount: 0,
  PageSize: 10,
  CurrentPage: 1,
  ItemCount: 0,
});
const currentPage = ref<number>(1);
const filter = ref<FormFilter>({});
const filter$ = new Subject<FormFilter>();

onMounted(() => {
  if (!props.defaultData || !props.defaultData.length) {
    requestData();
  }

  filter$.pipe(
      debounceTime(500)
  ).subscribe(filters => requestData(filters));
});

onUnmounted(() => {
  filter$.unsubscribe();
});

function requestData(filters?: FormFilter) {
  if (!props.dataRequest) {
    return;
  }

  const requestObject: BaseRequest = {
    Page: currentPage.value,
  };
  if (filters) {
    requestObject.Filters = filters;
  }

  props.dataRequest(requestObject)?.subscribe(res => {
    if (props.dataMapper) {
      data.value = props.dataMapper(res);
    } else {
      data.value = res;
    }
  });
}

const formattedRows = computed<FormattedRow[]>(() => {
  return data.value?.Items?.map((item: any) => {
    const row: FormattedRow = [];

    for (const value of Object.values(props.columns)) {
      let newValue = '';
      
      newValue = value.valueResolver ? value.valueResolver(item) : item[value.accessorKey] as string;

      const rowItem: FormattedRowItem = {
        value: newValue,
      };

      if (value.linkResolver) {
        rowItem.link = value.linkResolver(item);
      }
      row.push(rowItem);
    }

    return row;
  })
});

function onFilterFormChange(accessorKey: string, value: string | number) {
  const filterCopy = {...filter.value};
  if (filterCopy.hasOwnProperty(accessorKey)) {
    if (typeof value === 'string') {
      if (value) {
        filterCopy[accessorKey] = value;
      } else {
        delete filterCopy[accessorKey];
      }
    }
  } else {
    if (typeof value === "string") {
      filterCopy[accessorKey] = value;
    }
  }

  filter.value = filterCopy;
  filter$.next(filterCopy);
}

function toPreviousPage() {
  if (currentPage.value === 1) {
    return;
  }

  currentPage.value--;
  requestData(filter.value);
}

function toNextPage() {
  if (currentPage.value === data.value.PageCount) {
    return;
  }

  currentPage.value++;
  requestData(filter.value);
}

function toPage(pageNumber: number) {
  if (currentPage.value === pageNumber || data.value.PageCount < pageNumber) {
    return;
  }

  currentPage.value = pageNumber;
  requestData(filter.value);
}
</script>

<template>
  <div class="table-container">
    <div class="table-container-header">
      <p class="common-title">
        {{ title }}
      </p>
      <div v-if="buttons" class="buttons">
        <Button
          v-for="button in buttons"
          :key="button.label"
          :show="button.show"
          :on-click="button.onClick"
          :link="button.link"
          :label="button.label"
          theme="secondary"
        />
      </div>
    </div>
    
    <div class="common-bg table">
      <div class="table-row table-header">
        <div v-for="column in columns" :key="column.accessorKey" class="table-cell">
          <p>{{ column.header }}</p>
        </div>
      </div>
      <form v-if="columns.some(col => col.filter)" class="table-row table-filter">
        <div v-for="column in columns" :key="column.accessorKey" class="table-cell">
          <InputField
            v-if="column.filter"
            :field="{
              type: 'text',
              name: `${column.accessorKey}-filter`,
              placeholder: column.header,
              required: false
            }" 
            :on-value-change="value => onFilterFormChange(column.accessorKey, value)"
          />
        </div>
      </form>
      <template v-if="formattedRows?.length">
        <div v-for="(row, rowIndex) in formattedRows" :key="rowIndex" class="table-row">
          <div v-for="(item, itemIndex) in row" :key="itemIndex" class="table-cell">
            <router-link v-if="item.link" :to="item.link">
              {{ item.value }}
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </router-link>
            <p v-else>{{ item.value }}</p>
          </div>
        </div>
      </template>
      <div v-if="formattedRows?.length" class="table-pagination">
        <div class="pagination-inner">
          <button
            :disabled="currentPage === 1"
            @click="toPreviousPage()"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button v-if="currentPage > 1" @click="toPage(currentPage - 1)">
            <span>{{ currentPage - 1 }}</span>
          </button>
          <button class="active">
            <span>{{ currentPage }}</span>
          </button>
          <button v-if="currentPage < data.PageCount" @click="toPage(currentPage + 1)">
            <span>{{ currentPage + 1 }}</span>
          </button>
          <button :disabled="currentPage === data.PageCount" @click="toNextPage()">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
            </svg>
          </button>
        </div>
      </div>
      <p v-else class="no-data">{{ state.global.localization.Nui.Labels.NoDataToDisplay }}</p>
    </div>
  </div>
</template>

<style scoped lang="sass">
.table-container
  margin-bottom: 40px

  .table-container-header
    width: 100%
    display: flex
    flex-direction: row
    align-items: center
    justify-content: space-between
    margin-bottom: 15px

    .buttons
      display: flex
      flex-direction: row
      align-items: center
      justify-content: space-between

  .no-data
    font-size: 18px
    color: var(--clr-text-secondary)
    display: block
    padding-left: 10px

  .table
    padding: 20px 20px 20px 10px

    .table-row
      display: grid
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr))
      grid-gap: 10px
      border-radius: var(--radius-sm)
      &:hover:not(.table-header):not(.table-filter)
        background: var(--clr-bg-5)
      &:last-of-type:not(.table-header):not(.table-filter)
        margin-bottom: 0

      .table-cell
        padding: 5px 20px 5px 10px
        &:not(:last-of-type)
          border-right: 1px solid var(--clr-bg-40)
        &.numeric
          p
            font-variant-numeric: tabular-nums
      
        p, a
          font-size: 18px
          color: var(--clr-text-secondary)
          text-decoration: none

        a
          display: flex
          flex-direction: row
          align-items: center

          svg
            margin-left: 10px
            height: 20px
            color: var(--clr-text-tertiary)

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

    .table-header .table-cell
      padding-bottom: 10px

    .table-filter .table-cell
      padding-bottom: 15px

      .input-box
        margin: 0
            
  .table-pagination
    display: flex
    flex-direction: row
    align-items: center
    justify-content: flex-end
    padding-top: 15px
    margin-top: 20px
    border-top: 1px solid var(--clr-bg-40)
    
    .pagination-inner
      display: flex
      flex-direction: row
      align-items: center
    
      button
        display: flex
        flex-direction: row
        align-items: center
        justify-content: center
        width: 35px
        height: 40px
        border-radius: var(--radius-sm)
        background: transparent
        margin: 0 2px
        border: none
        &:hover:not(:disabled):not(.active)
          background: var(--clr-bg-80)
          cursor: pointer
        &:disabled
          svg, span
            color: var(--clr-text-tertiary)
        &.active
          background: var(--clr-bg-40)
        
        svg
          height: 22px
          color: var(--clr-text-primary)
          
        span
          font-size: 18px
          color: var(--clr-text-primary)

</style>

