import { ILocalization } from "./ILocalization";

export interface IBaseNuiEvent {
  type: string;
}

export interface MenuObjectsEvent extends IBaseNuiEvent {
  data: {
    Title: string;
    UseLocalization: boolean;
    Id: string;
    Items: {
      Title: string;
      Description: string;
      Hashcode: string;
      Type: number;
      Items?: string[];
    }[];
  }[];
}

export interface RadialMenuObjectsEvent extends IBaseNuiEvent {
  data: {
    Title: string;
    Id: string;
    Items: {
      Title: string;
      Hashcode: string;
    }[];
  }[];
}

export interface LocalizationChangeEvent extends IBaseNuiEvent {
  data: ILocalization;
}

export interface MenuShowEvent extends IBaseNuiEvent {
  menuId: string;
}

export interface RadialMenuControlEvent extends IBaseNuiEvent {
  control: 'select' | 'up' | 'down';
  x?: number;
  y: number;
}