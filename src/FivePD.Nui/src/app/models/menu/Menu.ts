import { MenuItem } from './MenuItem';

export class Menu {

  private readonly _hashcode: string;
  private readonly _title: string = '';
  private readonly _useLocalization: boolean = false;
  private readonly _items: MenuItem[] = [];

  constructor(hashcode: string, title: string, useLocalization: boolean, items: MenuItem[]) {
    this._hashcode = hashcode;
    this._title = title;
    this._useLocalization = useLocalization;
    this._items = items;
  }

  get hashcode(): string {
    return this._hashcode;
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
