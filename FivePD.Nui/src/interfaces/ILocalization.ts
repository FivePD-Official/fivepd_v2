export interface ILocalization {
  Title: string
  DutyMenu: {
    Title: string,
    OnDuty: {
      Title: string,
      Description: string,
    },
    GetLoadout: {
      Title: string,
      Description: string,
    },
    ChangeLanguage: {
      Title: string,
      Description: string,
    },
    SpawnVehicle: {
      Title: string,
      Description: string,
    }
  },
  Nui: {
    Labels: {
      Acronym: string,
      ActiveCalls: string,
      Address: string,
      AssignedCall: string,
      Callsign: string,
      CitizenDatabase: string,
      Dashboard: string,
      Department: string,
      Departments: string,
      Division: string,
      Divisions: string,
      Members: string,
      MyDetails: string,
      MyDepartment: string,
      Name: string,
      NoAssignedCall: string,
      NoDataToDisplay: string,
      NotInDepartment: string,
      Permissions: string,
      Rank: string,
      Ranks: string,
      Settings: string,
      Statistics: string,
      Status: string,
      Type: string,
      Units: string,
      VehicleDatabase: string,
    },
    Actions: {
      Kick: string,
      SetRank: string,
      SetCallsign: string,
    }
  }
}