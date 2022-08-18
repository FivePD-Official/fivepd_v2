import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MdtComponent } from './pages/mdt/mdt.component';
import { BlankComponent } from './pages/blank/blank.component';
import { MenuComponent } from './pages/menu/menu.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';

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
        component: DashboardComponent
      },
      {
        path: 'vehicle-database',
        component: DashboardComponent
      },
      {
        path: 'departments',
        component: DashboardComponent,
        children: [
          {
            path: ':id',
            component: DashboardComponent
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
