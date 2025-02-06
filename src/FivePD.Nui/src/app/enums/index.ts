export enum MenuItemType {
  Divider,
  Button,
  List,
  Toggle,
}

export enum DateFormat {
  DMY,
  MDY,
  YMD
}

export enum FGender {
  Male,
  Female,
}

export enum CoreSetId {
  ArrestReport = 'fivepd_arrest_report',
  IncidentReport = 'fivepd_incident_report',
  PedTicket = 'fivepd_ped_ticket',
  PedWarning = 'fivepd_ped_warning',
}

export type SetId = CoreSetId | string;

export enum FDutyStatus {
  OffDuty,
  OnDuty,
}

export enum FUnitAvailability {
  Receiving,
  NotReceiving,
}

export enum FPriority {
  Code1,
  Code2,
  Code3,
  Code99,
}

export enum FIncidentType {
  Callout,
  TrafficStop,
}

export enum PedLicenseType {
  Driving,
  Fishing,
  Hunting,
  Ccw,
}
