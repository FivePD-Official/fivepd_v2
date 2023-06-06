import { Injectable } from '@angular/core';
import { EventService } from '@services/event/event.service';
import {
  GetAllMenusResponse,
  Menu,
  MenuButtonItem,
  MenuCheckboxItem,
  MenuItem,
  MenuListItem,
  RadialMenu,
  RadialMenuObjectsEvent, TimerBarEvent,
  UpdateMenuItemEvent
} from '@models';
import { MenuItemType, NuiEventType } from '@enums';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  private _menus: Menu[] = [];
  private _radialMenus: RadialMenu[] = [];

  constructor(
    private _event: EventService,
    private _http: HttpClient,
  ) {
    // this._event.onEvent<RadialMenuObjectsEvent>(NuiEventType.SendRadialMenuObjects).subscribe(res => this.onRadialMenuObjectsEvent(res));
    this._event.onEvent<UpdateMenuItemEvent>(NuiEventType.UpdateMenuItem).subscribe(res => this.onUpdateMenuItemEvent(res));
  }

  load(): void {
    if (this._menus.length) {
      return;
    }

    this._http.post<GetAllMenusResponse>('fivepd:menu:getAll', {}).subscribe(menus => {
      this.createMenus(menus);
    });
  }

  getMenuById(id: string): Menu {
    return this._menus.find(menu => menu.id === id)!;
  }

  getRadialMenuById(id: string): RadialMenu {
    return this._radialMenus.find(menu => menu.Id === id)!;
  }

  private onRadialMenuObjectsEvent(res: RadialMenuObjectsEvent): void {
    this._radialMenus = res.data;
  }

  private createMenus(res: GetAllMenusResponse): void {
    const menus: Menu[] = [];
    for (const menu of res.Menus) {
      const menuItems: MenuItem[] = [];
      for (const item of menu.Items) {
        let _item: MenuItem | undefined;
        switch (item.Type) {
          case MenuItemType.Button: {
            _item = new MenuButtonItem(item.Title, item.Description, item.Hashcode);
            break;
          }
          case MenuItemType.List: {
            _item = new MenuListItem(item.Title, item.Description, item.Hashcode);
            (_item as MenuListItem).items = item.Items!;
            break;
          }
          case MenuItemType.Toggle: {
            _item = new MenuCheckboxItem(item.Title, item.Description, item.Hashcode);
            break;
          }
        }
        if (_item) {
          menuItems.push(_item as MenuItem);
        }
      }
      menus.push(new Menu(menu.Id, menu.Title, menu.UseLocalization, menuItems));
    }

    this._menus = menus;
  }

  private onUpdateMenuItemEvent(res: UpdateMenuItemEvent): void {
    for (const menu of this._menus) {
      for (const item of menu.items) {
        if (item.hashcode === res.hashcode) {
          if (Reflect.has(item, res.property)) {
            Reflect.set(item, res.property, res.value);
          }
          break;
        }
      }
    }
  }
}
