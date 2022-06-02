import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';

export class MenuCheckboxItem extends MenuItem {

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