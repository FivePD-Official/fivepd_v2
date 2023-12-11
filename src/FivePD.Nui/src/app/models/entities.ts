import {LooseDataAttribute} from './loose-data';
import {FGender, FIncidentPriority, FIncidentType, PedLicenseType} from "@enums";

export type Id = string | number;

export interface WithId {
  Id: number;
}

export interface WithStringId {
  Id: string;
}

export interface User extends WithId {
  FirstLogin: boolean;
  DismissedWelcomeMessage: boolean;
  Name: string;
  License: string;
  Role: number;
  Callsign: string;
  Department?: Department;
  Division?: Division;
  Rank?: Rank;
  DepartmentId?: number;
  DivisionId?: number;
  RankId?: number;
  Xp: number;
  FixedRank: boolean;
  PreferredLanguage: string;
}

export type SimpleUser = Pick<User, 'Id' | 'Name'>;

export interface DepartmentAreaCoordinate extends WithId {
  X: number;
  Y: number;
}

export interface Department extends WithId {
  Name: string;
  Callsign: string;
  CallsignFormat: string;
  MaxUsers: number;
  Users?: User[];
  Divisions?: Division[];
  Ranks?: Rank[];
  UserCount?: number;
  DefaultRank?: Rank;
  DefaultRankId?: number;
}

export interface SimpleDepartment extends WithId {
  Name: string;
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

export interface LooseDataEntity extends WithId {
  SetId: string;
  MappedValues: Record<string, any>;
  Set?: {
    Attributes?: LooseDataAttribute[];
  }
}

export interface MappedLooseDataEntity extends Omit<LooseDataEntity, 'MappedValues'> {
  [key: string]: any;
}

export interface LooseDataEntityWrapper {
  entity: MappedLooseDataEntity;
  attributes: LooseDataAttribute[];
}

export interface LooseDataEntityPageWrapper {
  page: Page<MappedLooseDataEntity>;
  attributes: LooseDataAttribute[];
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

export interface PedHistory extends WithId {
  Gender: FGender;
  Firstname: string;
  Lastname: string;
  Birthdate: string;
  Warrants: string[];
  Licenses: PedLicense[];
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

export type SimplePedHistory = Pick<PedHistory, 'Id' | 'Firstname' | 'Lastname' | 'Birthdate' | 'Gender'>

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

export interface FVehicle extends WithId {
  NetworkId: number;
  LicensePlate: string;
  Color: string;
  Brand: string;
}

export interface FVehicleDTO extends FVehicle {
  Owner?: PedHistory;
}
