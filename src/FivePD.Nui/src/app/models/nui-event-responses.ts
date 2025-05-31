import { User, Vector3 } from "./entities";
import { FDutyStatus, FPriority, FIncidentType, FUnitAvailability } from "@enums";
import { MenuListOption } from "./menu/MenuListItem";
import { MenuItem } from "./menu/MenuItem";

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
  Event: string;
}

export interface MenuItemDto {
  Hashcode: string;
  Title: string;
  Description: string;
  Enabled: boolean;
  Type: number;
  Divider: boolean;
  Items?: MenuListOption[];
}

export interface GetAllMenusResponse extends BaseNuiEvent {
  Menus: {
    Id: string;
    Title: string;
    Items: MenuItemDto[];
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
  Data: string;
}

export interface MenuShowEvent extends BaseNuiEvent {
  MenuId: string;
}

export interface RadialMenuControlEvent extends BaseNuiEvent {
  Control: 'select' | 'up' | 'down';
  X?: number;
  Y: number;
}

export interface UpdateMenuItemEvent extends BaseNuiEvent {
  Hashcode: string;
  Property: string;
  Value: any;
}

export interface UpdateMenuListOptionEvent extends UpdateMenuItemEvent {
  Index: number;
}

export interface AddMenuItemEvent {
  MenuId: string;
  Item: MenuItemDto;
}

export interface RemoveMenuItemEvent {
  MenuId: string;
  ItemHashcode: string;
}

export interface SoundEvent extends BaseNuiEvent {
  Sound: string;
}

export interface SoundSpeakEvent extends BaseNuiEvent {
  TextToPlay: string;
}

export interface TimerBarEvent extends BaseNuiEvent {
  Show: boolean;
  Title: string;
  TimeToHold: number;
}

export interface TimerBarProgressEvent extends BaseNuiEvent {
  CurrentProgress: number;
  TotalDuration: number;
}

export interface SimplePlayerDTO {
  NetworkId: string;
  Name: string;
}

export interface UnitDTO {
  Callsign: string;
  Players: SimplePlayerDTO[];
  DepartmentId: number;
  AssignedToIncident: boolean;
}

export interface PlayerDetailsDTO {
  NetworkId: string;
  User: User;
  DutyStatus: FDutyStatus;
  UnitCallsign: string;
}

interface ClientIncident {
  Id: string;
  Type: FIncidentType;
  Title: string;
  Description: string;
  Priority: FPriority;
  ActivityRadius: number;
  Location: {
    X: number;
    Y: number;
    Z: number;
  };
}

export interface IncidentWrapper {
  Incident: ClientIncident;
  Address: string;
}

export interface IncidentDTO {
  Id: string;
  Type: FIncidentType;
  Priority: FPriority;
  Title: string;
  Description: string;
  Address: string;
  Units: string[];
}

export interface GetSafeZoneSize {
  SafeZoneSize: number;
}

export interface InformationUpdateDTO extends BaseNuiEvent {
  PanicMode: boolean;
  Status: string;
  DutyStatus: FDutyStatus;
  Availability: FUnitAvailability;
  IncidentWrapper?: {
    Incident: {
      Id: string;
      Type: FIncidentType;
      Title: string;
      Description: string;
      Priority: FPriority;
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

export interface SimpleCallout {
  Id: string;
  Title: string;
  Description: string;
  Priority: FPriority;
  ActivityRadius: number;
  Location: Vector3;
  GeneratedAt: string;
}

export interface DispatcherUnitDTO extends UnitDTO {
  Status: string;
  Availability: FUnitAvailability;
  AssignedCallout?: SimpleCallout;
}

export interface CommonDispatcherUnitDTO extends UnitDTO {
  Status: string;
  Availability: FUnitAvailability;
}

export interface SimplePlayer {
  NetworkId: number;
  Position: Vector3;
}

export interface UpdatePlayersEvent extends BaseNuiEvent {
  Players: SimplePlayer[];
}

export interface OnUnitCreated extends BaseNuiEvent {
  Unit: CommonDispatcherUnitDTO;
}

export interface OnUnitRemoved extends BaseNuiEvent {
  Callsign: string;
}

export interface OnAudioSpeak extends BaseNuiEvent {
  Sound: string;
}

export interface OnAudioPlaySound extends BaseNuiEvent {
  Text: string;
}
