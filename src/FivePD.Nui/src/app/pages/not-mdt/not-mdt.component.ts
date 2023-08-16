import { Component } from '@angular/core';
import { MenuService } from "@services/menu/menu.service";
import { Menu, MenuShowEvent } from "@models";
import { EventService } from "@services/event/event.service";
import { WindowService } from "@services/window/window.service";
import { Router } from "@angular/router";

@Component({
  selector: 'app-not-mdt',
  templateUrl: './not-mdt.component.html',
  styleUrls: ['./not-mdt.component.sass']
})
export class NotMdtComponent {

  menu?: Menu;

  constructor(private readonly _menuService: MenuService,
              private readonly _eventService: EventService,
              public readonly windowService: WindowService,
              private readonly _router: Router) {
    this._eventService.onEvent<MenuShowEvent>('fivepd:ui:view:menu').subscribe(menu => {
      this.menu = this._menuService.getMenuById(menu.MenuId);
    });
  }

}
