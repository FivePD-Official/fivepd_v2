import { RouteRecordRaw } from 'vue-router';
import Dashboard from './views/Dashboard.vue';
import CitizenDatabaseIndex from './views/CitizenDatabase/Index.vue';
import CitizenDatabaseView from './views/CitizenDatabase/View.vue';
import VehicleDatabase from './views/VehicleDatabase.vue';
import Settings from './views/Settings.vue';
import DepartmentsIndex from './views/Departments/Index.vue';
import DepartmentsNew from './views/Departments/New.vue';
import DepartmentsView from './views/Departments/View.vue';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: Dashboard
  },
  {
    path: '/citizen-database',
    component: CitizenDatabaseIndex,
  },
  {
    path: '/citizen-database/:id',
    component: CitizenDatabaseView
  },
  {
    path: '/vehicle-database',
    component: VehicleDatabase
  },
  {
    path: '/settings',
    component: Settings
  },
  {
    path: '/departments',
    component: DepartmentsIndex,
  },
  {
    path: '/departments/new',
    component: DepartmentsNew,
  },
  {
    path: '/departments/:id',
    component: DepartmentsView,
  }
];
export default routes;