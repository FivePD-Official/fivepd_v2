import { Component } from '@angular/core';
import { GlobalState } from '@store';
import { IconType, User } from '@models';
import { Store } from '@ngrx/store';
import { toggleSidebarCollapse } from '@store/app.actions';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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
export class SidebarComponent {

  user?: User;
  isSidebarCollapsed: boolean = false;
  routes: Route[] = [
    {
      title: 'DISPATCH',
      path: () => '/mdt/dispatch',
      icon: IconType.Squares,
      show: () => true,
    },
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
      title: 'Developer tools',
      path: () => '/mdt/devtools',
      icon: IconType.CodeBracket,
      show: () => this._userService.userHasRole(UserRole.Admin, UserRole.Developer),
    }
  ];

  constructor(
    private readonly store: Store<GlobalState>,
    private readonly _userService: UserService,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(res => {
      this.isSidebarCollapsed = res.app.isSidebarCollapsed;
      this.user = res.user.details.User;
    });
  }

  toggleSidebarCollapse(): void {
    this.store.dispatch(toggleSidebarCollapse());
  }

}
