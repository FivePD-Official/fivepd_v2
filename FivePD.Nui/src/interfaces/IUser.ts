export interface IUser {
  Id: number,
  Name: string,
  License: string,
  Admin: boolean,
  Callsign: string,
  Department?: {
    Id: number,
    Name: string,
    Acronym: string,
    CallsignFormat: string,
    MaxUsers: number,
  },
  Division?: {
    Id: number,
    Name: string,
  },
  Rank?: {
    Id: number,
    Name: string,
  }
}
