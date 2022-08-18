import { IUser } from '@interfaces';

export class SetUser {
  static readonly type = '[user] set user';
  constructor(public payload: IUser) {}
}

export class TogglePanicMode {
  static readonly type = '[user] toggle panic mode';
  constructor(public payload: boolean) {}
}
