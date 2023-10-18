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
import { RadialMenuComponent } from './pages/radial-menu/radial-menu.component';
import { TimerBarComponent } from './pages/timer-bar/timer-bar.component';
import { SettingsComponent } from './pages/mdt/settings/settings.component';
import { ReportsComponent } from './pages/mdt/reports/reports.component';
import { DevtoolsGeneralComponent } from './pages/mdt/devtools/devtools-general/devtools-general.component';
import { DevtoolsCalloutsComponent } from './pages/mdt/devtools/devtools-callouts/devtools-callouts.component';
import { DevtoolsViewComponent } from './pages/mdt/devtools/devtools-view/devtools-view.component';
import { InformationComponent } from "./pages/information/information.component";
import { NotMdtComponent } from "./pages/not-mdt/not-mdt.component";
import { DispatchComponent } from "./pages/mdt/dispatch/dispatch.component";
import { IncidentHistoryComponent } from "./pages/mdt/incident-history/incident-history.component";
import {
  CitizenDatabaseIdComponent
} from "./pages/mdt/citizen-database/citizen-database-id/citizen-database-id.component";
import { MessagesComponent } from "./pages/mdt/messages/messages.component";
import { CalloutDetailsComponent } from "./pages/mdt/settings/callout-details/callout-details.component";

const mdtRoutes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: 'dispatch',
    component: DispatchComponent
  },
  {
    path: 'reports',
    component: ReportsComponent
  },
  {
    path: 'settings',
    children: [
      {
        path: '',
        component: SettingsComponent,
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
  },
  {
    path: 'devtools',
    component: DevtoolsViewComponent,
    children: [
      {
        path: '',
        component: DevtoolsGeneralComponent,
      },
      {
        path: 'callouts',
        component: DevtoolsCalloutsComponent
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
