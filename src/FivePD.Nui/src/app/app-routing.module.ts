import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MdtComponent } from './pages/mdt/mdt.component';
import { BlankComponent } from './pages/blank/blank.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';
import { DepartmentsViewComponent } from './pages/mdt/departments/departments-view/departments-view.component';
import {
  CitizenDatabaseIndexComponent
} from './pages/mdt/citizen-database/citizen-database-index/citizen-database-index.component';
import { RadialMenuComponent } from './pages/radial-menu/radial-menu.component';
import { TimerBarComponent } from './pages/timer-bar/timer-bar.component';
import { ReportsComponent } from './pages/mdt/reports/reports.component';
import { NotMdtComponent } from "./pages/not-mdt/not-mdt.component";
import { IncidentHistoryComponent } from "./pages/mdt/incident-history/incident-history.component";
import {
  CitizenDatabaseIdComponent
} from "./pages/mdt/citizen-database/citizen-database-id/citizen-database-id.component";
import { MessagesComponent } from "./pages/mdt/messages/messages.component";
import { CalloutDetailsComponent } from "./pages/mdt/settings/callout-details/callout-details.component";
import {
  VehicleDatabaseIndexComponent
} from "./pages/mdt/vehicle-database/vehicle-database-index/vehicle-database-index.component";
import {
  VehicleDatabaseIdComponent
} from "./pages/mdt/vehicle-database/vehicle-database-id/vehicle-database-id.component";

const mdtRoutes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: 'dispatch',
    loadComponent: () => import('./pages/mdt/dispatch/dispatch.component').then(m => m.DispatchComponent),
  },
  {
    path: 'reports',
    component: ReportsComponent
  },
  {
    path: 'settings',
    children: [
      {
        path: 'user',
        loadComponent: () => import('./pages/mdt/settings/user-settings/user-settings.component').then(m => m.UserSettingsComponent),
      },
      {
        path: 'admin',
        loadComponent: () => import('./pages/mdt/settings/admin-settings/admin-settings.component').then(m => m.AdminSettingsComponent),
      },
      {
        path: 'departments',
        children: [
          {
            path: 'area-management',
            loadComponent: () => import('./pages/mdt/settings/admin-settings/_components/department-settings-area/department-area-management.component').then(m => m.DepartmentAreaManagementComponent),
          },
          {
            path: ':id',
            component: DepartmentsViewComponent,
            data: {
              mode: 'edit'
            },
          },
        ]
      },
      {
        path: 'callout-details',
        children: [
          {
            path: ':id',
            component: CalloutDetailsComponent,
          },
        ],
      },
    ]
  },
  {
    path: 'messages',
    component: MessagesComponent,
  },
  {
    path: 'incidents',
    component: IncidentHistoryComponent
  },
  {
    path: 'citizen-database',
    children: [
      {
        path: '',
        component: CitizenDatabaseIndexComponent
      },
      {
        path: ':id',
        component: CitizenDatabaseIdComponent,
      }
    ]
  },
  {
    path: 'vehicle-database',
    children: [
      {
        path: '',
        component: VehicleDatabaseIndexComponent,
      },
      {
        path: ':id',
        component: VehicleDatabaseIdComponent,
      }
    ]
  },
  {
    path: 'departments',
    children: [
      {
        path: ':id',
        component: DepartmentsViewComponent,
        data: {
          mode: 'view'
        }
      }
    ]
  },
  {
    path: 'vehicle-settings',
    loadComponent: () => import('./pages/mdt/settings/vehicle-settings/vehicle-settings.component').then(m => m.VehicleSettingsComponent),
  },
  {
    path: 'devtools',
    loadComponent: () => import('./pages/mdt/devtools/devtools-view/devtools-view.component').then(m => m.DevtoolsViewComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/mdt/devtools/devtools-general/devtools-general.component').then(m => m.DevtoolsGeneralComponent),
      },
      {
        path: 'callouts',
        loadComponent: () => import('./pages/mdt/devtools/devtools-callouts/devtools-callouts.component').then(m => m.DevtoolsCalloutsComponent),
      }
    ]
  },
];

const routes: Routes = [
  {
    path: '',
    component: BlankComponent
  },
  {
    path: 'radial-menu/:id',
    component: RadialMenuComponent
  },
  {
    path: 'timer-bar',
    component: TimerBarComponent
  },
  {
    path: 'mdt',
    component: MdtComponent,
    children: mdtRoutes,
  },
  {
    path: 'not-mdt',
    component: NotMdtComponent,
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
