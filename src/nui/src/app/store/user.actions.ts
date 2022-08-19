import { User } from '@models';

export class SetUser {
  static readonly type = '[user] set user';
  constructor(public payload: User) {}
}

export class TogglePanicMode {
  static readonly type = '[user] toggle panic mode';
  constructor(public payload: boolean) {}
}
