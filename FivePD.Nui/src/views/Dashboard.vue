<script setup lang="ts">
import { ClientEvents } from "@enums";
import { ref } from "vue";
import { post } from "@utils";
import { IDepartment, IUser } from "@interfaces";
import { State } from "@store";
import { useStore } from "vuex";

const store = useStore<State>();

const departments = ref<IDepartment[]>([]);
const activeCalls = ref<any>([]); // TODO: request

post<IUser>(ClientEvents.GetPlayerDetails).subscribe((res) => {
  store.commit("setUserDetails", res);
});
post<IDepartment[]>(ClientEvents.ListDepartments).subscribe((res) => {
  departments.value = res;
});
</script>

<template>
  <PageHeader :title="store.state.user.localization.Nui.Labels.Dashboard" />

  <div class="grid">
    <div class="grid-item grid--details">
      <p class="common-title">{{ store.state.user.localization.Nui.Labels.MyDetails }}</p>
      <div class="common-bg">
        <ul>
          <li>
            <p>{{ store.state.user.localization.Nui.Labels.Department }}</p>
            <p>{{ store.state.user.details.Department?.Name ?? "-" }}</p>
          </li>
          <li>
            <p>{{ store.state.user.localization.Nui.Labels.Division }}</p>
            <p>{{ store.state.user.details.Division?.Name ?? "-" }}</p>
          </li>
          <li>
            <p>{{ store.state.user.localization.Nui.Labels.Rank }}</p>
            <p>-</p>
            <!-- TODO -->
          </li>
        </ul>
      </div>
    </div>

    <div class="grid-item">
      <p class="common-title">
        {{ store.state.user.localization.Nui.Labels.Statistics }}
      </p>
      <div class="common-bg">
        <!-- TODO -->
      </div>
    </div>

    <div class="grid-item">
      <p class="common-title">
        {{ store.state.user.localization.Nui.Labels.AssignedCall }}
      </p>
      <div class="common-bg">
        <!-- TODO -->
      </div>
    </div>
  </div>

  <Table
    :title="store.state.user.localization?.Nui.Labels.ActiveCalls"
    :rows="[]"
    :columns="{
      Type: store.state.user.localization?.Nui.Labels.Type,
      Address: store.state.user.localization?.Nui.Labels.Address,
      Status: store.state.user.localization?.Nui.Labels.Status,
      Units: store.state.user.localization?.Nui.Labels.Units,
    }"
  />

  <Table
    :title="store.state.user.localization?.Nui.Labels.Departments"
    :rows="departments"
    :columns="{
      Name: store.state.user.localization?.Nui.Labels.Name,
      Acronym: store.state.user.localization?.Nui.Labels.Acronym,
    }"
  />
</template>

<style scoped lang="sass">
.grid
  display: grid
  grid: 1fr / repeat(3, 1fr)
  gap: 40px
  margin-bottom: 40px

  .grid-item
    width: 100%

    .common-bg
      width: 100%
      height: 240px

  .grid--details
    height: 100%

    ul
      height: 100%
      list-style: none
      padding: 0
      display: flex
      flex-direction: column
      align-items: flex-start
      justify-content: space-between

      li
        p
          font-size: 18px
          font-weight: 600
          color: var(--clr-text-tertiary)
          margin-bottom: 2px

          + p
            font-size: 20px
            font-weight: 400
            color: var(--clr-text-primary)
</style>
