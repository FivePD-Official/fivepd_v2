import { FIncidentPriority } from "@enums";
import { Vector3 } from "./entities";

export interface DispatcherCalloutDTOClientWrapper {
  Callout: {
    Id: string;
    Title: string;
    Description: string;
    Priority: FIncidentPriority;
    Location: Vector3;
    GeneratedAt: string;
  };
  Address: string;
}

export interface CalloutRemoveFromDispatcher {
  CalloutId: string;
}
