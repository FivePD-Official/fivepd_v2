import { MenuItemType } from '@enums';

export abstract class MenuItem {

  public Title: string;
  public Enabled = true;
  public Description: string;

  private readonly _type: MenuItemType;
  private readonly _hashcode: string;
  private readonly _divider: boolean;

  protected constructor(title: string, description: string, hashcode: string, type: MenuItemType) {
    this.Title = title;
    this.Description = description;
    this._hashcode = hashcode;
    this._type = type;

    this._divider = type === MenuItemType.Divider;
  }

  get type(): MenuItemType {
    return this._type;
  }

  get hashcode(): string {
    return this._hashcode;
  }

  get divider(): boolean {
    return this._divider;
  }

  abstract onPress(): void;

  protected postRequest(path: string, index: any): void {
    fetch(`https://fivepd/${path}`, {
      body: index as unknown as BodyInit,
      method: 'POST',
    }).catch((err) => console.error('FivePD Menu error:', err));
  }

}
