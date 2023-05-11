import { Component, OnDestroy, OnInit } from '@angular/core';
import { GlobalState } from '@store';
import { IconType, User } from '@models';
import { Store } from '@ngrx/store';
import { toggleSidebarCollapse } from '@store/app.actions';
import { Subject, takeUntil } from 'rxjs';

interface Route {
  title: string;
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
      title: 'UI.LABEL.DASHBOARD',
      path: () => '/mdt/dashboard',
      icon: IconType.Squares,
      show: () => true,
    },
    {
      title: 'UI.LABEL.CITIZEN_DATABASE',
      path: () => '/mdt/citizen-database',
      icon: IconType.Identification,
      show: () => true,
    },
    {
      title: 'UI.LABEL.VEHICLE_DATABASE',
      path: () => '/mdt/vehicle-database',
      icon: IconType.Truck,
      show: () => true,
    },
    {
      title: 'UI.LABEL.REPORT.PLURAL',
      path: () => '/mdt/reports',
      icon: IconType.Report,
      show: () => true,
    },
    {
      title: 'UI.LABEL.SETTINGS',
      path: () => '/mdt/settings',
      icon: IconType.Cog,
      show: () => true,
    },
    {
      title: 'UI.LABEL.MY_DEPARTMENT',
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
