import { User, Vector3 } from "./entities";
import { FDutyStatus, FIncidentPriority, FPlayerAvailability } from "@enums";

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

interface ClientCallout {
  Id: string;
  Title: string;
  Description: string;
  Priority: FIncidentPriority;
  ActivityRadius: number;
  Location: {
    X: number;
    Y: number;
    Z: number;
  };
}

export interface ClientCalloutWrapper {
  Callout: ClientCallout;
  Address: string;
}

export interface CalloutDTO {
  Title: string;
  Description: string;
  Address: string;
  Units: string[];
}

export interface GetSafeZoneSize {
  SafeZoneSize: number;
}

export interface InformationUpdateDTO extends BaseNuiEvent {
  Status: string;
  DutyStatus: FDutyStatus;
  Availability: FPlayerAvailability;
  CalloutWrapper?: {
    Callout: {
      Id: string;
      Title: string;
      Description: string;
      Priority: FIncidentPriority;
      ActivityRadius: number;
      Location: Vector3;
    };
    Address: string;
  }
}

export interface CalloutTypeDTO {
  TypeName: string;
  AssemblyName: string;
}
