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
  Acronym: string,
  CallsignFormat: string,
  MaxUsers: number,
  Users?: IUser[],
  Divisions?: IDivision[],
  Ranks?: IRank[],
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
