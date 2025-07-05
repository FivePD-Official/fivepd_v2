import { Component, inject } from '@angular/core';

import { RouterLink, RouterLinkActive, RouterOutlet } from "@angular/router";
import { PageHeaderComponent } from "../../../components/page-header/page-header.component";
import { IconType, Routes } from "@models";
import { UserRole, UserService } from "@services/user/user.service";
import { IconComponent } from "../../../components/icon/icon.component";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { RouteHelper } from "../../_service/route-helper";

@Component({
    selector: 'app-settings',
    imports: [RouterOutlet, PageHeaderComponent, IconComponent, TranslatePipe, RouterLinkActive, RouterLink],
    templateUrl: './settings.component.html',
    styleUrl: './settings.component.sass'
})
export class SettingsComponent {

  private readonly userService = inject(UserService);
  protected readonly routeHelper = inject(RouteHelper);

  readonly routes: Routes = [
    {
      title: 'UI.LABEL.USER.PLURAL',
      path: () => `/mdt/settings/users`,
      icon: IconType.Users,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'UI.LABEL.DEPARTMENT.PLURAL',
      path: () => `/mdt/settings/departments`,
      icon: IconType.Building2,
      show: () => true,
    },
    {
      title: 'Ranks',
      path: () => '/mdt/settings/ranks',
      icon: IconType.Star,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'UI.LABEL.CALLOUT.PLURAL',
      path: () => `/mdt/settings/callouts`,
      icon: IconType.Megaphone,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'UI.LABEL.ATTRIBUTE.PLURAL',
      path: () => `/mdt/settings/attributes`,
      icon: IconType.AtSymbol,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'UI.LABEL.VEHICLE.PLURAL',
      path: () => '/mdt/settings/vehicles',
      icon: IconType.Truck,
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
    },
    {
      title: 'UI.LABEL.LOCATION.PLURAL',
      path: () => '/mdt/settings/locations',
      icon: IconType.MapPin,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
    {
      title: 'UI.LABEL.LOADOUT.PLURAL',
      path: () => '/mdt/settings/loadouts',
      icon: IconType.Briefcase,
      show: () => this.userService.userHasRole(UserRole.Admin),
    },
  ];
}
