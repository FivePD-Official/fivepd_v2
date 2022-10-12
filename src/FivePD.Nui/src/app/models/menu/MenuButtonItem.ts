import { MenuItemType } from '@enums';
import { MenuItem } from './MenuItem';
import { MenuListItem } from './MenuListItem';

export class MenuButtonItem extends MenuItem {

  constructor(title: string, description: string, hashcode: string) {
    super(title, description, hashcode, MenuItemType.Button);
  }

  override onPress() {
    this.postRequest(`fivepd:menu:onButtonSelect:${this.hashcode}`, -1);
  }

}
