import { SetAttribute } from './loose-data';

export interface WithId {
  Id: number;
}
export interface User extends WithId {
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

export interface Division extends WithId {
  Name: string;
}

export interface Rank extends WithId {
  Name: string;
  Permission: {
    Kick: boolean;
    SetRank: boolean;
    SetCallsign: boolean;
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
}

export interface MappedLooseDataEntity extends Omit<LooseDataEntity, 'MappedValues'> {
  [key: string]: any;
}

export interface LooseDataEntityWrapper {
  entity: MappedLooseDataEntity;
  attributes: SetAttribute[];
}

export interface LooseDataEntityPageWrapper {
  page: Page<MappedLooseDataEntity>;
  attributes: SetAttribute[];
}

export enum FDutyStatus {
  OffDuty,
  OnDuty,
}

export enum FPlayerAvailability {
  Receiving,
  NotReceiving,
}

export enum FIncidentPriorty {
  Code1,
  Code2,
  Code3,
}
