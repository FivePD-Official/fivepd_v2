import { MenuItem } from './MenuItem';

export class Menu {

  private readonly _id: string;
  private readonly _title: string = '';
  private readonly _items: MenuItem[] = [];

  constructor(id: string, title: string, items: MenuItem[]) {
    this._id = id;
    this._title = title;
    this._items = items;
  }

  get id(): string {
    return this._id;
  }

  get title(): string {
    return this._title;
  }

  get items(): MenuItem[] {
    return this._items;
  }

}
