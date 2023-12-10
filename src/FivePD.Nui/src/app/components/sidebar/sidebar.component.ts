import {Component} from '@angular/core';
import {GlobalState} from '@store';
import {GeneralConfig, IconType, User} from '@models';
import {Store} from '@ngrx/store';
import {UserRole, UserService} from '@services/user/user.service';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {DispatchService} from "@services/dispatch/dispatch.service";
import {ConfigService} from "@services/config/config.service";
import {TranslatePipe} from '../../pipes/translate.pipe';
import {IconComponent} from '../icon/icon.component';
import {RouterLink, RouterLinkActive} from '@angular/router';
import {NgClass} from '@angular/common';
import {AppActions} from "@store/app.actions";

export interface Route {
  title: string;
  path: () => string;
  icon: IconType;
  show: () => boolean;
}


type RouteOrDivider = Route | { divider: true; };

type Routes = RouteOrDivider[];

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
  ],
})
export class SidebarComponent {


  config?: GeneralConfig;
  user?: User;
  isSidebarCollapsed: boolean = false;
  routes: Routes = [
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
      icon: IconType.Library,
      show: () => !!this.user?.Department ?? false,
    },
    {
      divider: true,
    },
    {
      title: 'UI.LABEL.MY_SETTINGS',
      path: () => '/mdt/settings/user',
      icon: IconType.User,
      show: () => true,
    },
    {
      title: 'UI.LABEL.SETTINGS',
      path: () => '/mdt/settings/admin',
      icon: IconType.Cog,
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
    },
    {
      title: 'UI.LABEL.VEHICLE_SETTINGS',
      path: () => '/mdt/vehicle-settings',
      icon: IconType.Truck,
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
    },
    {
      title: 'Developer tools',
      path: () => '/mdt/devtools',
      icon: IconType.CodeBracket,
      show: () => this.userService.userHasRole(UserRole.Admin, UserRole.Developer),
    }
  ];

  constructor(
    public readonly userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _dispatchService: DispatchService,
    private readonly _configService: ConfigService,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(res => {
      this.isSidebarCollapsed = res.app.isSidebarCollapsed;
      this.user = res.user.details.User;
    });

    this._configService.getGeneralConfig().pipe(takeUntilDestroyed()).subscribe(config => {
      this.config = config;
    });
  }

  isRoute(route: RouteOrDivider): route is Route {
    return !('divider' in route);
  }

  toggleSidebarCollapse(): void {
    this._store.dispatch(AppActions.toggleSidebarCollapse());
  }

  enableDispatchMode(): void {
    this._dispatchService.enable();
  }

  protected readonly UserRole = UserRole;
}
