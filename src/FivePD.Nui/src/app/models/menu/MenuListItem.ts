import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';

export class MenuListItem extends MenuItem {

  private _currentIndex = 0;

  public Options: MenuListOption[] = [];

  constructor(title: string, description: string, enabled: boolean, hashcode: string) {
    super(title, description, enabled, hashcode, MenuItemType.List);
  }

  get currentIndex(): number {
    return this._currentIndex;
  }

  next(): void {
    if (this._currentIndex === this.Options.length - 1) return;
    this._currentIndex++;
    this.postRequest(`fivepd:menu:onListIndexChange:${this.hashcode}`, this.currentIndex);
  }

  prev(): void {
    if (this._currentIndex === 0) return;
    this._currentIndex--;
    this.postRequest(`fivepd:menu:onListIndexChange:${this.hashcode}`, this.currentIndex);
  }

  override onPress() {
    this.postRequest(`fivepd:menu:onListSelect:${this.hashcode}`, this.currentIndex);
  }

}

export class MenuListOption {
  public Title: string;
  public Enabled = true;

  constructor(title: string, enabled: boolean = true) {
    this.Title = title;
    this.Enabled = enabled;
  }
}
