export interface ILocalization {
  Title: string
  DutyMenu: {
    Title: string
    OnDuty: {
      Title: string
      Description: string
    },
    GetLoadout: {
      Title: string
      Description: string
    },
    ChangeLanguage: {
      Title: string
      Description: string
    },
    SpawnVehicle: {
      Title: string
      Description: string
    }
  },
  TrafficStop: {
    Initiate: string
    WaitUntilStop: string
    Proceed: string
    Canceled: string
    StoppedVehicle: string
  },
  Error: {
    MultipleMenusWithSameKey: string
  },
  Warning: {
    ConfigException: string
    EmptyConfigException: string
  },
  Nui: {
    Labels: {
      Acronym: string
      AcronymHelpLabel: string
      ActiveCalls: string
      Address: string
      AssignedCall: string
      AvailableCharacters: string
      Callsign: string
      CallsignFormat: string
      CallsignFormatHelpLabel: string
      CitizenDatabase: string
      Dash: string
      Dashboard: string
      Department: string
      Departments: string
      Division: string
      Divisions: string
      Letter: string
      MaxNumberOfMembers: string
      MaxNumberOfMembersHelpLabel: string
      Members: string
      MyDetails: string
      MyDepartment: string
      Name: string
      NewDepartment: string
      NoAssignedCall: string
      NoDataToDisplay: string
      NotInDepartment: string
      Number: string
      Permissions: string
      Rank: string
      Ranks: string
      Settings: string
      Statistics: string
      Status: string
      Type: string
      Units: string
      VehicleDatabase: string
    },
    Actions: {
      Join: string
      Kick: string
      Leave: string
      Save: string
      SetRank: string
      SetCallsign: string
    },
    Errors: {
      DatabaseAccess: string
      DepartmentExistsWithName: string
    }
  }
}

export const defaultLocalization = {
  Title: '',
  DutyMenu: {
    Title: '',
    OnDuty: {
      Title: '',
      Description: '',
    },
    GetLoadout: {
      Title: '',
      Description: '',
    },
    ChangeLanguage: {
      Title: '',
      Description: '',
    },
    SpawnVehicle: {
      Title: '',
      Description: '',
    }
  },
  TrafficStop: {
    Initiate: '',
    WaitUntilStop: '',
    Proceed: '',
    Canceled: '',
    StoppedVehicle: '',
  },
  Error: {
    MultipleMenusWithSameKey: '',
  },
  Warning: {
    ConfigException: '',
    EmptyConfigException: '',
  },
  Nui: {
    Labels: {
      Acronym: '',
      AcronymHelpLabel: '',
      ActiveCalls: '',
      Address: '',
      AssignedCall: '',
      AvailableCharacters: '',
      Callsign: '',
      CallsignFormat: '',
      CallsignFormatHelpLabel: '',
      CitizenDatabase: '',
      Dash: '',
      Dashboard: '',
      Department: '',
      Departments: '',
      Division: '',
      Divisions: '',
      Letter: '',
      MaxNumberOfMembers: '',
      MaxNumberOfMembersHelpLabel: '',
      Members: '',
      MyDetails: '',
      MyDepartment: '',
      Name: '',
      NewDepartment: '',
      NoAssignedCall: '',
      NoDataToDisplay: '',
      NotInDepartment: '',
      Number: '',
      Permissions: '',
      Rank: '',
      Ranks: '',
      Settings: '',
      Statistics: '',
      Status: '',
      Type: '',
      Units: '',
      VehicleDatabase: '',
    },
    Actions: {
      Join: '',
      Kick: '',
      Leave: '',
      Save: '',
      SetRank: '',
      SetCallsign: '',
    },
    Errors: {
      DatabaseAccess: '',
      DepartmentExistsWithName: '',
    }
  }
}