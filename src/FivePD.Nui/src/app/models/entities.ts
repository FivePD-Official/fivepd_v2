import { SetAttribute } from './loose-data';

export interface User {
  Id: number;
  Name: string;
  License: string;
  Admin: boolean;
  Callsign: string;
  Department?: Department;
  Division?: Division;
  Rank?: Rank;
  DepartmentId?: number;
  DivisionId?: number;
  RankId?: number;
}

export interface Department {
  Id: number;
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

export interface Division {
  Id: number;
  Name: string;
}

export interface Rank {
  Id: number;
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

export interface LooseDataEntity {
  [key: string]: any;
  Id: number;
  SetId: string;
}

export interface LooseDataEntityWrapper {
  entity: LooseDataEntity;
  attributes: SetAttribute[];
}

export interface LooseDataEntityPageWrapper {
  page: Page<LooseDataEntity>;
  attributes: SetAttribute[];
}
