import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { GlobalState } from '@store';
import { Store } from '@ngxs/store';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { ToggleSidebarCollapse } from '@store/app.actions';
import { TogglePanicMode } from '@store/user.actions';
import { IconType } from '@models';

interface Route {
  title: string;
  path: string;
  icon: IconType;
  show: boolean;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.sass'],
  // encapsulation: ViewEncapsulation.None,
})
export class SidebarComponent implements OnInit {

  @Dispatch() togglePanicMode = () => new TogglePanicMode(!this.isPanicModeActive);
  @Dispatch() toggleSidebarCollapse = () => new ToggleSidebarCollapse(!this.isSidebarCollapsed);

  isSidebarCollapsed: boolean = false;
  isPanicModeActive: boolean = false;
  routes: Route[] = [
    {
      title: 'Dashboard',
      path: '/mdt/dashboard',
      icon: IconType.Squares,
      show: true,
    },
    {
      title: 'CitizenDatabase',
      path: '/mdt/citizen-database',
      icon: IconType.Identification,
      show: true,
    },
    {
      title: 'VehicleDatabase',
      path: '/mdt/vehicle-database',
      icon: IconType.Truck,
      show: true,
    },
    {
      title: 'Settings',
      path: '/mdt/settings',
      icon: IconType.Cog,
      show: true,
    },
    {
      title: 'MyDepartment',
      path: `/mdt/departments/${0}`,
      icon: IconType.Users,
      show: true,
    },
  ];

  constructor(
    private _store: Store,
  ) {
    this._store.subscribe((res: GlobalState) => {
      this.isSidebarCollapsed = res.app.isSidebarCollapsed;
      this.isPanicModeActive = res.user.isPanicModeActive;
    });
  }

  ngOnInit(): void {
  }

}
