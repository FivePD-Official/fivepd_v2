export interface IUser {
  [key: string]: any,
  Id: number,
  Name: string,
  License: string,
  Admin: boolean,
  Callsign: string,
  Department?: IDepartment,
  Division?: IDivision,
  Rank?: IRank,
  DepartmentId?: number,
  DivisionId?: number,
  RankId?: number,
}

export interface IDepartment {
  Id: number,
  Name: string,
  Callsign: string,
  CallsignFormat: string,
  MaxUsers: number,
  Users?: IUser[],
  Divisions?: IDivision[],
  Ranks?: IRank[],
  UserCount?: number,
  DefaultRank?: IRank,
  DefaultRankId?: number,
}

export interface IDivision {
  Id: number,
  Name: string,
}

export interface IRank {
  Id: number,
  Name: string,
  Permission: {
    Kick: boolean,
    SetRank: boolean,
    SetCallsign: boolean,
  }
}
