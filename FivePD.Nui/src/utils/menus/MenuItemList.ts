import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';

export class MenuItemList extends MenuItem {

  private _items: string[] = [];
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

  set setItems(items: string[]) {
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