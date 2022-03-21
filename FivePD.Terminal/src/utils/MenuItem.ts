import { MenuItemType } from '@enums';
import { Post } from './request';

export class Menu {

  private _id: string;
  private _title: string;
  private _useLocalization: boolean;
  private _items: MenuItem[];

  constructor(id: string, title: string, useLocalization: boolean, items: MenuItem[]) {
    this._id = id;
    this._title = title;
    this._useLocalization = useLocalization;
    this._items = items;
  }

  get id(): string {
    return this._id;
  }

  get title(): string {
    return this._title;
  }

  get useLocalization(): boolean {
    return this._useLocalization;
  }

  get items(): MenuItem[] {
    return this._items;
  }

}

export class MenuItem {

  private _type: MenuItemType;
  private _title: string;
  private _description: string;
  private _hashcode: string;

  constructor(title: string, description: string, hashcode: string, type: MenuItemType) {
    this._title = title;
    this._description = description;
    this._hashcode = hashcode;
    this._type = type;
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

  get hashcode(): string {
    return this._hashcode;
  }

  onPress(): void {
    if (this._type === MenuItemType.List) {
      Post(`fivepd:menu:trigger_item_${this._hashcode}`, {
        index: ((this as unknown) as MenuItemList).currentIndex
      }).catch(() => {});
    } else {
      Post(`fivepd:menu:trigger_item_${this._hashcode}`).catch(() => {});
    }
  }

}

export class MenuItemButton extends MenuItem {

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.Button);
  }

}

export class MenuItemToggle extends MenuItem {

  private _isChecked: boolean = false;

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.Toggle);
  }

  get checked(): boolean {
    return this._isChecked;
  }

  toggle(): void {
    this._isChecked = !this._isChecked;
  }

}

export class MenuItemList extends MenuItem {

  private _items: any[] = [];
  private _currentIndex: number = 0;

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.List);
  }

  get items(): any[] {
    return this._items;
  }

  get currentIndex(): number {
    return this._currentIndex;
  }

  set setItems(items: any[]) {
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