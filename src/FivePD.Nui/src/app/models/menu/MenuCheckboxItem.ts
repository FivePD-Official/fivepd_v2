import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';
import { MenuListItem } from './MenuListItem';

export class MenuCheckboxItem extends MenuItem {

  public Checked = false;

  constructor(title: string, description: string, enabled: boolean, hashcode: string) {
    super(title, description, enabled, hashcode, MenuItemType.Toggle);
  }

  toggle(): void {
    this.Checked = !this.Checked;
  }

  override onPress() {
    this.postRequest(`fivepd:menu:onCheckboxSelect:${this.hashcode}`, this.Checked);
  }

}
