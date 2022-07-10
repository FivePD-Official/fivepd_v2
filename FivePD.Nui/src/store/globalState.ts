import type { IMenu, ILocalization, IListItem } from "@interfaces";
import { Menu } from "@utils";

export interface GlobalState {
  radialMenus: IMenu[],
  menus: Menu[],
  availableLocales: IListItem[],
  localization: ILocalization,
}

export const globalState = {
  state: (): GlobalState => {
    return {
      radialMenus: [],
      menus: [],
      availableLocales: [],
      localization: defaultLocalization,
    }
  },
  mutations: {
    setRadialMenus(state: GlobalState, radialMenus: IMenu[]) {
      state.radialMenus = radialMenus;
    },
    setMenus(state: GlobalState, menus: Menu[]) {
      state.menus = menus;
    },
    setAvailableLocales(state: GlobalState, availableLocales: IListItem[]) {
      state.availableLocales = availableLocales;
    },
    setLocalization(state: GlobalState, localization: ILocalization) {
      state.localization = localization;
    },
  },
  actions: {}
};

const defaultLocalization = {
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
  Nui: {
    Labels: {
      Acronym: '',
      AcronymHelpLabel: '',
      ActiveCalls: '',
      Address: '',
      AssignedCall: '',
      Callsign: '',
      CallsignFormat: '',
      CitizenDatabase: '',
      Dashboard: '',
      Department: '',
      Departments: '',
      Division: '',
      Divisions: '',
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