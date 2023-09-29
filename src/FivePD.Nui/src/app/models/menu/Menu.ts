import { MenuItem } from './MenuItem';
import { BehaviorSubject } from "rxjs";

export class Menu {

  private readonly _id: string;
  private readonly _title: string = '';
  items: MenuItem[] = [];

  readonly currentItemIndex$ = new BehaviorSubject<number>(0);

  constructor(id: string, title: string, items: MenuItem[]) {
    this._id = id;
    this._title = title;
    this.items = items;
  }

  get id(): string {
    return this._id;
  }

  get title(): string {
    return this._title;
  }

}
