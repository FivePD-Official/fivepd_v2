import { MenuItemType } from '@enums';

export class MenuItem {

  private _type: MenuItemType;
  private _title: string;
  private _description: string;
  private _onPress: (obj: MenuItem) => void | Promise<void> = () => {};

  constructor(title: string, description: string, type: MenuItemType, onPress?: () => void | Promise<void>) {
    this._title = title;
    this._description = description;
    this._type = type;

    if (onPress) this._onPress = onPress;
  }

  get type(): MenuItemType {
    return this._type;
  }

  get title(): string {
    return this._title;
  }

  get description(): string {
    return this._description;
  }

  onPress(): void {
    this._onPress(this);
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

  get checked(): boolean {
    return this._isChecked;
  }

  toggle(): void {
    this._isChecked = !this._isChecked;
  }

}

export class MenuItemList extends MenuItem {

  private _items: string[] | number[] = [];
  private _currentIndex: number = 0;

  constructor(title: string, description: string, onPress?: () => void | Promise<void>) {
    super(title, description, MenuItemType.List, onPress);
  }

  get items(): string[] | number[] {
    return this._items;
  }

  get currentIndex(): number {
    return this._currentIndex;
  }

  set setItems(items: string[] | number[]) {
    this._items = items;
  }

  next(): void {
    if (this._currentIndex === this._items.length-1) return;
    this._currentIndex++;
  }

  prev(): void {
    if (this._currentIndex === 0) return;
    this._currentIndex--;
  }

}