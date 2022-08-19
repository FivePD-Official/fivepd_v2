import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';

export class MenuListItem extends MenuItem {

  private _currentIndex = 0;

  public items: string[] = [];

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.List);
  }

  get currentIndex(): number {
    return this._currentIndex;
  }

  next(): void {
    if (this._currentIndex === this.items.length-1) return;
    this._currentIndex++;
    this.postRequest(`fivepd:menu:list_onchange_${this.hashcode}`, this.currentIndex);
  }

  prev(): void {
    if (this._currentIndex === 0) return;
    this._currentIndex--;
    this.postRequest(`fivepd:menu:list_onchange_${this.hashcode}`, this.currentIndex);
  }

}
