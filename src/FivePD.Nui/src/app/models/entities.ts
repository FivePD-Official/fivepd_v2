import {VariableDataAttribute} from './loose-data';
import {FGender, FIncidentPriority, FIncidentType, PedLicenseType} from "@enums";

export type Id = string | number;

export interface WithId {
  Id: number;
}

export interface WithStringId {
  Id: string;
}

export interface UserCallsignFormat extends WithId {
  Values: string[];
}

export interface User extends WithId {
  FirstLogin: boolean;
  DismissedWelcomeMessage: boolean;
  Name: string;
  License: string;
  Role: number;
  Callsign: string;
  CallsignFormat: UserCallsignFormat;
  Department?: Department;
  Division?: Division;
  Rank?: Rank;
  DepartmentId?: number;
  DivisionId?: number;
  RankId?: number;
  AudioVolume: number;
  Xp: number;
  FixedRank: boolean;
  PreferredLanguage: string;
  AccessFivePD: boolean;
}

export type SimpleUser = Pick<User, 'Id' | 'Name'>;

export type SimpleWeapon = Pick<Weapon, 'Id' | 'Name'>

export type SimpleWeaponComponent = Pick<WeaponComponent, 'HashKey' | 'Name'>

export interface DepartmentAreaCoordinate extends WithId {
  X: number;
  Y: number;
}

export enum CallsignFormatItemType {
  Dash,
  FixedCharacter,
  RandomLetter,
  RandomNumber,
  Word,
}

export interface CallsignFormatItem {
  Type: CallsignFormatItemType;
  Value?: string;
}

export interface DepartmentRank {
  DepartmentId: number;
  Rank: Rank;
  RankId: number;
}

export interface Department extends WithId {
  Name: string;
  Callsign: string;
  CallsignFormat: CallsignFormatItem[];
  UseGlobalRanks: boolean;
  MaxUsers: number;
  Users?: User[];
  Divisions?: Division[];
  DepartmentRanks?: DepartmentRank[];
  UserCount?: number;
  DefaultRank?: Rank;
  DefaultRankId?: number;
}

export interface SimpleDepartment extends WithId {
  Name: string;
}

export interface VehicleConfig extends WithId {
  Type: VehicleConfigType;
  Name: string;
  Model: string;
  Livery: number;
  Extras: number[];
  Options: Record<string, any>;
  // Department: User["DepartmentId"]; TODO
  // Division: Division["Name"]; TODO
}

export interface LoadoutConfig extends WithId {
  Name: string;
  LoadoutWeapons: [];
  WeaponIds: number[];
  // Department: User["DepartmentId"]; TODO
  // Division: Division["Name"]; TODO
}

export interface Weapon extends WithId {
  Name: string;
  Model: string;
  WeaponWeaponComponents: [];
  WeaponComponentHashes: [];
  // Department: User["DepartmentId"]; TODO
  // Division: Division["Name"]; TODO
}

export interface WeaponComponent extends WithId {
  Name: string;
  HashKey: string;
  // Department: User["DepartmentId"]; TODO
  // Division: Division["Name"]; TODO
}

export enum VehicleConfigType {
  Police,
  Ambulance,
  TowTruck,
}

export interface Location extends WithId {
  Name: string;
  Type: LocationType;
  Position: Vector3;
}

export enum LocationType {
  MdtAccess,
  JailDropOff,
  LoadoutSelector,
  VehicleSelector,
}

export interface SimpleDepartmentWithArea extends SimpleDepartment {
  Callsign: string;
  Coordinates: DepartmentAreaCoordinate[];
}

export interface Division extends WithId {
  Name: string;
}

export interface Rank extends WithId {
  Name: string;
  RequiredXp: number;
  Global: boolean;
  Permission: {
    Kick: boolean;
    SetRank: boolean;
    SetDivision: boolean;
    DepartmentManager: boolean;
  };
}

export interface Page<T> {
  CurrentPage: number;
  Items: T[];
  PageCount: number;
  PageSize: number;
  ItemCount: number;
}

export interface VariableDataEntity extends WithId {
  SetId: string;
  MappedValues: Record<string, any>;
  Set?: {
    Attributes?: VariableDataAttribute[];
  }
}

export interface MappedVariableDataEntity extends Omit<VariableDataEntity, 'MappedValues'> {
  [key: string]: any;
}

export interface VariableDataEntityWrapper {
  entity: MappedVariableDataEntity;
  attributes: VariableDataAttribute[];
}

export interface VariableDataEntityPageWrapper {
  page: Page<MappedVariableDataEntity>;
  attributes: VariableDataAttribute[];
}

export interface Vector3 {
  X: number;
  Y: number;
  Z: number;
}

export interface IncidentHistory {
  Id: string;
  IncidentId: string;
  Title: string;
  Description: string;
  Location: string;
  Priority: FIncidentPriority;
  Type: FIncidentType,
  GeneratedAt: string;
  Users: IncidentHistoryUser[];
}

export interface IncidentHistoryUser {
  Id: number;
  User: User;
  IncidentHistory: IncidentHistory;
}

export type SimpleIncidentHistory = Pick<IncidentHistory, 'Id' | 'Title' | 'GeneratedAt'>;

export interface PedData extends WithId {
  Gender: FGender;
  Firstname: string;
  Lastname: string;
  Birthdate: string;
  Warrants: string[];
  Licenses: PedLicense[];
  VehicleData?: VehicleData;
}

export enum PedLicenseStatus {
  Valid,
  Expired,
  Suspended,
  Revoked,
}

export interface PedLicense extends WithStringId {
  Type: PedLicenseType;
  IssueDate: string;
  ExpirationDate: string;
  Status: PedLicenseStatus;
}

export type SimplePedHistory = Pick<PedData, 'Id' | 'Firstname' | 'Lastname' | 'Birthdate' | 'Gender'>

export interface Message {
  Id: number;
  Subject: string;
  Content: string;
  Sender: string;
  SentAt: string;
  Read: boolean;
}

export enum CalloutDetailsRulesetType {
  Allow,
  Deny,
}

export interface CalloutDetails extends WithId {
  Active: boolean;
  TypeName: string;
  AssemblyName: string;
  RulesetType: CalloutDetailsRulesetType;
}

export interface CalloutDetailsWithDepartments extends CalloutDetails {
  CalloutDetailsDepartments: {
    CalloutDetailsId: number;
    DepartmentId: number;
  }[];
}

export enum VehicleDocumentStatus {
  Valid,
  Expired,
  Suspended,
  Revoked,
}

export enum VehicleDocumentType {
  Registration,
  Insurance,
}

export interface VehicleDocument {
  Status: VehicleDocumentStatus;
  Type: VehicleDocumentType;
  ExpirationDate: string;
  IssueDate: string;
}

export interface VehicleData extends WithId {
  LicensePlate: string;
  Color: string;
  Brand: string;
  Documents: VehicleDocument[];
  PedData?: PedData;
}

export interface SpeedZone {
  Position: Vector3;
  Radius: number;
  Speed: number;
}

export interface SpeedZoneWithPlayerDTO extends WithStringId {
  SpeedZone: SpeedZone;
  PlayerName: string;
}
