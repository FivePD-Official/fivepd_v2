import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';
import { MenuListItem } from './MenuListItem';

export class MenuCheckboxItem extends MenuItem {

  public checked = false;

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.Toggle);
  }

  toggle(): void {
    this.checked = !this.checked;
  }

  override onPress() {
    this.postRequest(`fivepd:menu:onCheckboxSelect:${this.hashcode}`, this.checked);
  }

}
