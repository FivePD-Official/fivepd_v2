import { ILocalization } from "./ILocalization";

export interface IBaseNuiEvent {
  type: string;
}

export interface MenuObjectsResponse extends IBaseNuiEvent {
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

export interface RadialMenuObjectsResponse extends IBaseNuiEvent {
  data: {
    Title: string;
    Id: string;
    Items: {
      Title: string;
      Hashcode: string;
    }[];
  }[];
}

export interface LocalizationChangeResponse extends IBaseNuiEvent {
  data: ILocalization;
}

export interface ViewEventResponse extends IBaseNuiEvent {
  menuId?: string;
}