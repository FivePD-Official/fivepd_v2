export * from './NuiEventType';

export enum MenuItemType {
  Button,
  List,
  Toggle
}

export enum DateFormat {
  DMY,
  MDY,
  YMD
}

export enum Gender {
  Male,
  Female,
}

export enum DefaultSetId {
  ArrestReport = 'fivepd_arrest_report',
}

export type SetId = DefaultSetId | string;

export enum FDutyStatus {
  OffDuty,
  OnDuty,
}

export enum FUnitAvailability {
  Receiving,
  NotReceiving,
}

export enum FIncidentPriority {
  Code1,
  Code2,
  Code3,
}
