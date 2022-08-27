import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MdtComponent } from './pages/mdt/mdt.component';
import { BlankComponent } from './pages/blank/blank.component';
import { MenuComponent } from './pages/menu/menu.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';
import { DepartmentsViewComponent } from './pages/mdt/departments/departments-view/departments-view.component';
import {
  CitizenDatabaseIndexComponent
} from './pages/mdt/citizen-database/citizen-database-index/citizen-database-index.component';

const routes: Routes = [
  {
    path: '',
    component: BlankComponent
  },
  {
    path: 'menu/:id',
    component: MenuComponent
  },
  // {
  //   path: 'radial-menu/:id',
  //   component: MenuComponent
  // },
  {
    path: 'mdt',
    component: MdtComponent,
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'settings',
        component: DashboardComponent
      },
      {
        path: 'citizen-database',
        children: [
          {
            path: '',
            component: CitizenDatabaseIndexComponent
          }
        ]
      },
      {
        path: 'vehicle-database',
        component: DashboardComponent
      },
      {
        path: 'departments',
        children: [
          {
            path: ':id',
            component: DepartmentsViewComponent
          }
        ]
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
