import { Injectable } from '@angular/core';
import { EventService } from '@services/event/event.service';
import {
  AddMenuItemEvent,
  GetAllMenusResponse, MenuItemDto,
  Menu,
  MenuButtonItem,
  MenuCheckboxItem,
  MenuItem,
  MenuListItem, MenuListOption,
  RadialMenu,
  RadialMenuObjectsEvent, RemoveMenuItemEvent,
  UpdateMenuItemEvent, UpdateMenuListOptionEvent
} from '@models';
import { MenuItemType } from '@enums';
import { HttpClient } from '@angular/common/http';
import { MenuDividerItem } from "../../models/menu/MenuDividerItem";

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
    this._event.onEvent<UpdateMenuItemEvent>('fivepd:menu:update_item').subscribe(res => this.onUpdateMenuItemEvent(res));
    this._event.onEvent<UpdateMenuListOptionEvent>('fivepd:menu:update_menu_list_option').subscribe(res => this.onUpdateMenuListOptionEvent(res));
    this._event.onEvent<AddMenuItemEvent>('fivepd:menu:add_menu_item').subscribe(res => this.onAddMenuItemEvent(res));
    this._event.onEvent<RemoveMenuItemEvent>('fivepd:menu:remove_menu_item').subscribe(res => this.onRemoveMenuItemEvent(res));
  }

  load(): void {
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
        let _item = this.createItem(item);
        if (_item) {
          menuItems.push(_item as MenuItem);
        }
      }
      menus.push(new Menu(menu.Id, menu.Title, menuItems));
    }

    this._menus = menus;
  }

  private onUpdateMenuItemEvent(res: UpdateMenuItemEvent): void {
    for (const menu of this._menus) {
      for (const item of menu.items) {
        if (item.hashcode === res.Hashcode) {
          if (Reflect.has(item, res.Property)) {
            if (Array.isArray(res.Value))
            {
              const items = res.Value.map(x => new MenuListOption(x.Title, x.Enabled));
              Reflect.set(item, res.Property, items);
            }
            else
            {
              Reflect.set(item, res.Property, res.Value);
            }
          }
          break;
        }
      }
    }
  }

  private onUpdateMenuListOptionEvent(res: UpdateMenuListOptionEvent): void {
    for (const menu of this._menus) {
      for (const item of menu.items) {
        if (item.hashcode === res.Hashcode) {
          const list = item as MenuListItem;
          const option = list.Options[res.Index]
          if (Reflect.has(option, res.Property)) {
            Reflect.set(option, res.Property, res.Value);
          }
          break;
        }
      }
    }
  }

  private onAddMenuItemEvent(res: AddMenuItemEvent): void {
    const menu = this._menus.find(x => x.id === res.MenuId);
    if (!menu) {
      return;
    }

    const item = this.createItem(res.Item);
    if (!item) {
      return;
    }

    menu.items.push(item);
  }

  private onRemoveMenuItemEvent(res: RemoveMenuItemEvent): void {
    const menu = this._menus.find(x => x.id === res.MenuId);
    if (!menu) {
      return;
    }

    const itemIndex = menu.items.findIndex(x => x.hashcode === res.ItemHashcode);
    if (itemIndex === -1) {
      return;
    }

    menu.items.splice(itemIndex, 1);
    menu.currentItemIndex$.next(0);
  }

  private createItem(item: MenuItemDto): MenuItem | undefined {
    let _item: MenuItem | undefined;
    switch (item.Type) {
      case MenuItemType.Divider: {
        _item = new MenuDividerItem(item.Title);
        break;
      }
      case MenuItemType.Button: {
        _item = new MenuButtonItem(item.Title, item.Description, item.Enabled, item.Hashcode);
        break;
      }
      case MenuItemType.List: {
        _item = new MenuListItem(item.Title, item.Description, item.Enabled, item.Hashcode);
        (_item as MenuListItem).Options = item.Items!.map(x => new MenuListOption(x.Title, x.Enabled));
        break;
      }
      case MenuItemType.Toggle: {
        _item = new MenuCheckboxItem(item.Title, item.Description, item.Enabled, item.Hashcode);
        break;
      }
    }

    return _item;
  }

}
