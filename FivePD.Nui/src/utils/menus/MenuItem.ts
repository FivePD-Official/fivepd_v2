import { MenuItemType } from "@enums";
import { post } from "@utils";
import { MenuItemList } from "./MenuItemList";

export class MenuItem {

  private _type: MenuItemType;
  private _title: string;
  private _description: string;
  private _hashcode: string;

  constructor(title: string, description: string, hashcode: string, type: MenuItemType) {
    this._title = title;
    this._description = description;
    this._hashcode = hashcode;
    this._type = type;
  }

  get type(): MenuItemType {
    return this._type;
  }

  get title(): string {
    return this._title;
  }

  get description(): string {
    return this._description;
  }

  get hashcode(): string {
    return this._hashcode;
  }

  onPress(): void {
    if (this._type === MenuItemType.List) {
      const menuItemList = ((this as unknown)) as MenuItemList;
      if (menuItemList.items.length) {
        post(`fivepd:menu:trigger_item_${this._hashcode}`, menuItemList.currentIndex).subscribe();
      }
    } else {
      post(`fivepd:menu:trigger_item_${this._hashcode}`, -1).subscribe();
    }
  }

}