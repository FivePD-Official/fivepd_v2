import { Component } from '@angular/core';
import { GlobalState } from '@store';
import { GeneralConfig, IconType, User } from '@models';
import { Store } from '@ngrx/store';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DispatchService } from "@services/dispatch/dispatch.service";
import { ConfigService } from "@services/config/config.service";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../icon/icon.component';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgClass, NgTemplateOutlet } from '@angular/common';
import { RouteOrDivider, Routes, RouteWithChildren, RouteWithoutChildren } from "../../models/sidebar";
import { SidebarActions } from "@store/sidebar.actions";

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.sass'],
  standalone: true,
  imports: [
    RouterLinkActive,
    RouterLink,
    IconComponent,
    NgClass,
    TranslatePipe,
    NgTemplateOutlet
  ],
})
export class SidebarComponent {

  config?: GeneralConfig;
  user?: User;
  isSidebarCollapsed: boolean = false;
  private readonly defaultRoutes: Routes = [
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
      title: 'UI.LABEL.INCIDENT.PLURAL',
      path: () => '/mdt/incidents',
      icon: IconType.Report,
      show: () => true,
    },
    {
      title: 'UI.LABEL.MESSAGE.PLURAL',
      path: () => '/mdt/messages',
      icon: IconType.Envelope,
      show: () => this.config ? this.config.canSendMessageAll() : true,
    },
    {
      title: 'UI.LABEL.MY_DEPARTMENT',
      path: () => `/mdt/departments/${this.user?.DepartmentId}`,
      icon: IconType.Building,
      show: () => !!this.user?.Department ?? false,
    },
    {
      divider: true,
    },
    {
      title: 'UI.LABEL.MY_SETTINGS',
      path: () => `/mdt/my-settings`,
      icon: IconType.User,
      show: () => true,
    },
    {
      title: 'UI.LABEL.GLOBAL_SETTINGS',
      icon: IconType.Cog,
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
      children: [
        {
          title: 'Users',
          path: () => `/mdt/settings/users`,
          icon: IconType.Users,
          show: () => this.userService.userHasRole(UserRole.Admin),
        },
        {
          title: 'Departments',
          path: () => `/mdt/settings/departments`,
          icon: IconType.Building2,
          show: () => true,
        },
        {
          title: 'Callouts',
          path: () => `/mdt/settings/callouts`,
          icon: IconType.Megaphone,
          show: () => this.userService.userHasRole(UserRole.Admin),
        },
        {
          title: 'Attributes',
          path: () => `/mdt/settings/attributes`,
          icon: IconType.AtSymbol,
          show: () => this.userService.userHasRole(UserRole.Admin),
        },
        {
          title: 'Vehicle settings',
          path: () => '/mdt/settings/vehicles',
          icon: IconType.Truck,
          show: () => this.userService.userIsDepartmentManagerOrAdmin(),
        },
        {
          title: 'Locations',
          path: () => '/mdt/settings/locations',
          icon: IconType.MapPin,
          show: () => this.userService.userHasRole(UserRole.Admin),
        },
      ]
    },
    {
      title: 'Developer tools',
      path: () => '/mdt/devtools',
      icon: IconType.CodeBracket,
      show: () => this.userService.userHasRole(UserRole.Admin, UserRole.Developer),
    }
  ];
  routes: Routes = [];

  constructor(
    public readonly userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _dispatchService: DispatchService,
    private readonly _configService: ConfigService,
  ) {
    _store.dispatch(SidebarActions.setRoutes({routes: this.defaultRoutes}));
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.isSidebarCollapsed = state.sidebar.collapsed;
      this.routes = state.sidebar.routes;
      this.user = state.user.details.User;
    });

    this._configService.getGeneralConfig().pipe(takeUntilDestroyed()).subscribe(config => {
      this.config = config;
    });
  }

  isRoute(route: RouteOrDivider): route is RouteWithoutChildren {
    return !('divider' in route);
  }

  isDivider(route: RouteOrDivider): route is RouteWithoutChildren {
    return !this.isRoute(route);
  }

  hasChildren(route: RouteOrDivider): route is RouteWithChildren {
    return 'children' in route;
  }

  toggleRoute(route: RouteWithChildren): void {
    this._store.dispatch(SidebarActions.toggleRoute({route}));
  }

  toggleSidebarCollapse(): void {
    this._store.dispatch(SidebarActions.toggleCollapse());
  }

  enableDispatchMode(): void {
    this._dispatchService.enable();
  }

  protected readonly UserRole = UserRole;
  protected readonly IconType = IconType;
}
