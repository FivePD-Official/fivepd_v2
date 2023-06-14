import { FDutyStatus, User } from "./entities";

export interface ActionResponse {
  Status: boolean;
  Title: string;
  Description: string;
  Params: Record<string, string>;
}

export interface NameTakenResponse {
  Taken: boolean;
}

export interface BaseNuiEvent {
  type: string;
}

export interface GetAllMenusResponse extends BaseNuiEvent {
  Menus: {
    Id: string;
    Title: string;
    UseLocalization: boolean;
    Items: {
      Hashcode: string;
      Title: string;
      Description: string;
      Type: number;
      Items?: string[];
    }[];
  }[];
}

export interface RadialMenuObjectsEvent extends BaseNuiEvent {
  data: {
    Title: string;
    Id: string;
    Items: {
      Title: string;
      Hashcode: string;
    }[];
  }[];
}

export interface LocalizationChangeEvent extends BaseNuiEvent {
  data: string;
}

export interface MenuShowEvent extends BaseNuiEvent {
  menuId: string;
}

export interface RadialMenuControlEvent extends BaseNuiEvent {
  control: 'select' | 'up' | 'down';
  x?: number;
  y: number;
}

export interface UpdateMenuItemEvent extends BaseNuiEvent {
  hashcode: string;
  property: string;
  value: any;
}

export interface SoundEvent extends BaseNuiEvent {
  sound: string;
}

export interface SoundSpeakEvent extends BaseNuiEvent {
  textToPlay: string;
}

export interface TimerBarEvent extends BaseNuiEvent {
  show: boolean;
  title: string;
  timeToHold: number;
}

export interface SimplePlayerDTO {
  NetworkId: string;
  Name: string;
}

export interface UnitDTO {
  Callsign: string;
  Players: SimplePlayerDTO[];
}

export interface PlayerDetailsDTO {
  NetworkId: string;
  User: User;
  DutyStatus: FDutyStatus;
}

export interface CalloutDTO {
  Title: string;
  Description: string;
  Address: string;
  Units: string[];
}
