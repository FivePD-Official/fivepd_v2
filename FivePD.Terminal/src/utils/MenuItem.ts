import { MenuItemType } from '../enums';

export class MenuItem {

  type: MenuItemType;
  title: string;
  description: string;
  onPress: () => void | Promise<void> = () => {};

  constructor(title: string, description: string, type: MenuItemType, onPress?: () => void | Promise<void>) {
    this.title = title;
    this.description = description;
    this.type = type;

    if (onPress) this.onPress = onPress;
  }

}

export class MenuItemButton extends MenuItem {

  constructor(title: string, description: string, onPress?: () => void | Promise<void>) {
    super(title, description, MenuItemType.Button, onPress);
  }

}

export class MenuItemToggle extends MenuItem {

  private _isChecked: boolean = false;

  constructor(title: string, description: string, onPress?: () => void | Promise<void>) {
    super(title, description, MenuItemType.Toggle, onPress);
  }

  get checked() {
    return this._isChecked;
  }

  toggle(): void {
    this._isChecked = !this._isChecked;
  }

}

export class MenuItemList extends MenuItem {

  private _items: any[] = [];
  private _currentIndex: number = 0;

  constructor(title: string, description: string, onPress?: () => void | Promise<void>) {
    super(title, description, MenuItemType.List, onPress);
  }

  get currentIndex() {
    return this._currentIndex;
  }

  get items() {
    return this._items;
  }

  set setItems(items: any[]) {
    this._items = items;
  }

}