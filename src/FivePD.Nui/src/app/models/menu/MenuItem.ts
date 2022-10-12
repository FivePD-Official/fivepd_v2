import { MenuItemType } from '@enums';
import { MenuListItem } from './MenuListItem';

export abstract class MenuItem {

  public title: string;
  public enabled = true;

  private readonly _type: MenuItemType;
  private readonly _description: string;
  private readonly _hashcode: string;

  protected constructor(title: string, description: string, hashcode: string, type: MenuItemType) {
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

  abstract onPress(): void;

  protected postRequest(path: string, index: any): void {
    fetch(`https://fivepd/${path}`, {
      body: index as unknown as BodyInit,
      method: 'POST',
    }).catch((err) => console.error('FivePD Menu error:', err));
  }

}
