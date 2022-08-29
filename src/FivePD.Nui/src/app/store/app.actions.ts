import { IListItem } from './app.state';
import { Localization } from '@models';

export class SetAvailableLocales {
  static readonly type = '[app] set available locales';
  constructor(public payload: IListItem[]) {}
}

export class SetLocalization {
  static readonly type = '[app] set localization';
  constructor(public payload: Localization) {}
}

export class ToggleSidebarCollapse {
  static readonly type = '[app] toggle sidebar collapse';
  constructor(public payload: boolean) {}
}

export class SetMdtPath {
  static readonly type = '[app] set mdt path';
  constructor(public payload: string[]) {}
}
