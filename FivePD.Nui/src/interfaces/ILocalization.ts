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
      AcronymHelpLabel: string,
      ActiveCalls: string,
      Address: string,
      AssignedCall: string,
      AvailableCharacters: string,
      Callsign: string,
      CallsignFormat: string,
      CallsignFormatHelpLabel: string,
      CitizenDatabase: string,
      Dash: string,
      Dashboard: string,
      Department: string,
      Departments: string,
      Division: string,
      Letter: string,
      Divisions: string,
      MaxNumberOfMembers: string,
      MaxNumberOfMembersHelpLabel: string,
      Members: string,
      MyDetails: string,
      MyDepartment: string,
      Name: string,
      NewDepartment: string,
      NoAssignedCall: string,
      NoDataToDisplay: string,
      NotInDepartment: string,
      Number: string,
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
      Join: string,
      Kick: string,
      Leave: string,
      Save: string,
      SetRank: string,
      SetCallsign: string,
    },
    Errors: {
      DatabaseAccess: string,
      DepartmentExistsWithName: string,
    }
  }
}