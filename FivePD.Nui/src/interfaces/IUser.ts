export interface IUser {
  Id: number,
  Name: string,
  License: string,
  Admin: boolean,
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
  }
}
