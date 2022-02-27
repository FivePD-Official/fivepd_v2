<script setup lang="ts">
import { ISearchSubmitField } from '../../interfaces';

const onSearchSubmit = (fields: ISearchSubmitField[]) => {
  console.log(fields)
}

const fields= [
  {
    type: 'text',
    name: 'lastname',
    label: 'Lastname',
    placeholder: 'John'
  },
  {
    type: 'text',
    name: 'firstname',
    label: 'Firstname',
    placeholder: 'Doe'
  },
  {
    type: 'date',
    name: 'birthdate',
    label: 'Birthdate'
  }
];
const citizens: any[] = [];
for (let i = 0; i < 100; i++)
  citizens.push({ id: i, firstname: 'John', lastname: 'Doe', birthdate: '12 / 03 / 1998', wanted: false });
</script>

<template>
  <PageHeader title="Citizen database" />
  <div class="content">

    <SearchPanel
      :fields="fields"
      title="Search"
      subtitle="All fields are optional. If empty, every citizen will be displayed."
      buttonText="Search"
      @onSubmit="onSearchSubmit"
    />

    <div class="list" v-if="citizens?.length">
      <div class="list-item list-header">
        <p>Firstname</p>
        <p>Lastname</p>
        <p>Birthdate</p>
        <p>Wanted</p>
      </div>
      <router-link v-for="citizen in citizens" :to="`/citizen-database/${citizen.id}`" class="list-item">
        <p>{{ citizen.firstname }}</p>
        <p>{{ citizen.lastname }}</p>
        <p>{{ citizen.birthdate }}</p>
        <p :class="citizen.wanted ? 'text-danger' : ''">{{ citizen.wanted ? 'WANTED' : 'Not wanted' }}</p>
      </router-link>
    </div>
    <p v-else>There are no citizens in the system.</p>

  </div>
</template>

<style scoped lang="sass">
.content
  display: grid
  grid: 1fr / 300px 1fr
  gap: 80px
  z-index: var(--layer-0)

  .list
    display: grid
    grid-template-columns: 1fr
    grid-auto-flow: row
    gap: 5px
    width: 100%

    .list-item
      display: grid
      grid: 1fr / repeat(4, 1fr)
      height: 40px
      border-radius: var(--radius)
      align-items: center
      padding: 0 20px
      transition: none
      border: 1px solid transparent
      cursor: pointer
      text-decoration: none
      &:nth-child(even)
        background: var(--clr-bg-5)
      &:hover
        background: var(--clr-bg-40)
      &.list-header
        position: sticky
        top: var(--page-header-height)
        background: var(--clr-bg)
        p
          font-weight: 700
          color: var(--clr-text-primary)

      p
        font-size: 16px
        color: var(--clr-text-secondary)
        &.text-danger
          color: var(--clr-red)
          font-weight: 700

</style>
