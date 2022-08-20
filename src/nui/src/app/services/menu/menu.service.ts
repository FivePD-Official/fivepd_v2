import { Injectable } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { MenuObjectsEvent, RadialMenu, RadialMenuObjectsEvent, UpdateMenuItemEvent } from '@models';
import { MenuItemType, NuiEventType } from '@enums';
import { Menu, MenuButtonItem, MenuCheckboxItem, MenuItem, MenuListItem } from '@models';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  private _menus: Menu[] = [];
  private _radialMenus: RadialMenu[] = [];

  constructor(
    private _event: EventService,
  ) {
    this._event.onEvent<RadialMenuObjectsEvent>(NuiEventType.SendRadialMenuObjects).subscribe(res => this.onRadialMenuObjectsEvent(res));
    this._event.onEvent<UpdateMenuItemEvent>(NuiEventType.UpdateMenuItem).subscribe(res => this.onUpdateMenuItemEvent(res));
    this._event.onEvent<MenuObjectsEvent>(NuiEventType.SendMenuObjects).subscribe(res => this.setMenusEvent(res));
  }

  getMenuById(id: string): Menu {
    console.log('menus', this._menus)
    return this._menus.find(menu => menu.id === id)!;
  }

  private onRadialMenuObjectsEvent(res: RadialMenuObjectsEvent): void {
    this._radialMenus = res.data;
  }

  private setMenusEvent(res: MenuObjectsEvent): void {
    const menus: Menu[] = [];
    for (const menu of res.data) {
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
    console.log('menu creation', res, menus)
    this._menus = menus;
  }

  private onUpdateMenuItemEvent(res: UpdateMenuItemEvent): void {
    for (const menu of this._menus) {
      for (const item of menu.items) {
        if (item.hashcode === res.hashcode) {
          if (Reflect.has(item, res.fieldname)) {
            Reflect.set(item, res.fieldname, res.value);
          }
          break;
        }
      }
    }
  }
}
