import { MenuItemType } from '@enums';
import { MenuItem } from '@models';

export class MenuDividerItem extends MenuItem {

  constructor(title: string) {
    super(title, '', '', MenuItemType.Divider);
  }

  override onPress() {
  }

}
