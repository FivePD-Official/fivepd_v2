import { MenuItemType } from '@enums';
import { MenuListItem } from './MenuListItem';

export class MenuItem {

  public title: string;
  public enabled = true;

  private readonly _type: MenuItemType;
  private readonly _description: string;
  private readonly _hashcode: string;

  constructor(title: string, description: string, hashcode: string, type: MenuItemType) {
    this.title = title;
    this._description = description;
    this._hashcode = hashcode;
    this._type = type;
  }

  get type(): MenuItemType {
    return this._type;
  }

  get description(): string {
    return this._description;
  }

  get hashcode(): string {
    return this._hashcode;
  }

  onPress(): void {
    if (this._type === MenuItemType.List) {
      const menuItemList = ((this as unknown)) as MenuListItem;
      if (menuItemList.items.length) {
        this.postRequest(`fivepd:menu:onListItemSelect:${this._hashcode}`, menuItemList.currentIndex);
      }
    } else {
      this.postRequest(`fivepd:menu:onItemSelect:${this._hashcode}`, -1);
    }
  }

  protected postRequest(path: string, index: any): void {
    fetch(`https://fivepd/${path}`, {
      body: index as unknown as BodyInit,
      method: 'POST',
    }).catch((err) => console.error('FivePD Menu error:', err));
  }

}
