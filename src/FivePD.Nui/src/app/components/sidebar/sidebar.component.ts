import { Component } from '@angular/core';
import { GlobalState } from '@store';
import { GeneralConfig, IconType, Routes, User } from '@models';
import { Store } from '@ngrx/store';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DispatchService } from "@services/dispatch/dispatch.service";
import { ConfigService } from "@services/config/config.service";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../icon/icon.component';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { NgClass, NgTemplateOutlet } from '@angular/common';
import { SidebarActions } from "@store/sidebar.actions";
import { RouteHelper } from "../../pages/_service/route-helper";

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.sass'],
    imports: [
        RouterLinkActive,
        RouterLink,
        IconComponent,
        NgClass,
        TranslatePipe,
        NgTemplateOutlet
    ]
})
export class SidebarComponent {

  config?: GeneralConfig;
  user?: User;
  isSidebarCollapsed: boolean = false;
  private readonly defaultRoutes: Routes = [
    {
      title: 'LABEL.DASHBOARD.TITLE',
      path: () => '/mdt/dashboard',
      icon: IconType.Squares,
      show: () => true,
    },
    {
      title: 'LABEL.CITIZEN_DATABASE.TITLE',
      path: () => '/mdt/citizen-database',
      icon: IconType.Identification,
      show: () => true,
    },
    {
      title: 'LABEL.VEHICLE_DATABASE.TITLE',
      path: () => '/mdt/vehicle-database',
      icon: IconType.Truck,
      show: () => true,
    },
    {
      title: 'LABEL.REPORT.PLURAL',
      path: () => '/mdt/reports',
      icon: IconType.Report,
      show: () => true,
    },
    {
      title: 'LABEL.INCIDENT.PLURAL',
      path: () => '/mdt/incidents',
      icon: IconType.Report,
      show: () => true,
    },
    {
      title: 'LABEL.MESSAGE.PLURAL',
      path: () => '/mdt/messages',
      icon: IconType.Envelope,
      show: () => this.config ? this.config.canSendMessageAll() : true,
    },
    {
      title: 'LABEL.MY_DEPARTMENT',
      path: () => `/mdt/departments/${this.user?.DepartmentId}`,
      icon: IconType.Building,
      show: () => !!this.user?.Department,
    },
    {
      divider: true,
    },
    {
      title: 'LABEL.SCENE_MANAGEMENT.TITLE',
      path: () => '/mdt/scene-management',
      icon: IconType.Warning,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'LABEL.MY_SETTINGS.TITLE',
      path: () => `/mdt/my-settings`,
      icon: IconType.User,
      show: () => true,
    },
    {
      title: 'LABEL.GLOBAL_SETTINGS',
      icon: IconType.Cog,
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
      onClick: () => void this.router.navigate(['mdt', 'settings', 'users']),
      linkActive: () => this.router.url.startsWith('/mdt/settings'),
    },
    {
      title: 'LABEL.DEVELOPER_TOOLS.TITLE',
      path: () => '/mdt/devtools',
      icon: IconType.CodeBracket,
      show: () => this.userService.userHasRole(UserRole.Admin, UserRole.Developer),
    },
  ];
  routes: Routes = [];

  constructor(
    public readonly userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _dispatchService: DispatchService,
    private readonly _configService: ConfigService,
    private readonly router: Router,
    protected readonly routeHelper: RouteHelper
  ) {
    _store.dispatch(SidebarActions.setRoutes({routes: this.defaultRoutes}));
    _store.subscribe(state => {
      this.isSidebarCollapsed = state.sidebar.collapsed;
      this.routes = state.sidebar.routes;
      this.user = state.user.details.User;
    });

    this._configService.getGeneralConfig().pipe(takeUntilDestroyed()).subscribe(config => {
      this.config = config;
    });
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
