import { Component, OnDestroy, OnInit } from '@angular/core';
import { GlobalState } from '@store';
import { IconType, Localization, Paths, User } from '@models';
import { Store } from '@ngrx/store';
import { toggleSidebarCollapse } from '@store/app.actions';
import { Subject, takeUntil } from 'rxjs';

interface Route {
  title: Paths<Localization>;
  path: () => string;
  icon: IconType;
  show: () => boolean;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.sass'],
})
export class SidebarComponent implements OnDestroy {

  destroy$ = new Subject<void>();
  user?: User;
  isSidebarCollapsed: boolean = false;
  routes: Route[] = [
    {
      title: 'Nui.Labels.Dashboard',
      path: () => '/mdt/dashboard',
      icon: IconType.Squares,
      show: () => true,
    },
    {
      title: 'Nui.Labels.CitizenDatabase',
      path: () => '/mdt/citizen-database',
      icon: IconType.Identification,
      show: () => true,
    },
    {
      title: 'Nui.Labels.VehicleDatabase',
      path: () => '/mdt/vehicle-database',
      icon: IconType.Truck,
      show: () => true,
    },
    {
      title: 'Nui.Labels.Reports',
      path: () => '/mdt/reports',
      icon: IconType.Report,
      show: () => true,
    },
    {
      title: 'Nui.Labels.Settings',
      path: () => '/mdt/settings',
      icon: IconType.Cog,
      show: () => true,
    },
    {
      title: 'Nui.Labels.MyDepartment',
      path: () => `/mdt/departments/${this.user?.DepartmentId}`,
      icon: IconType.Users,
      show: () => !!this.user?.Department ?? false,
    },
    {
      // @ts-ignore
      title: 'Developer tools',
      path: () => '/mdt/devtools',
      icon: IconType.CodeBracket,
      show: () => this.user?.Admin ?? false,
    }
  ];

  constructor(
    private readonly store: Store<GlobalState>,
  ) {
    this.store.pipe(takeUntil(this.destroy$)).subscribe(res => {
      this.isSidebarCollapsed = res.app.isSidebarCollapsed;
      this.user = res.user.details;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.unsubscribe();
  }

  toggleSidebarCollapse(): void {
    this.store.dispatch(toggleSidebarCollapse());
  }

}
