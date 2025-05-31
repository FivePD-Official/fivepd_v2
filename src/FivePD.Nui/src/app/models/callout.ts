import { FPriority } from "@enums";
import { Vector3 } from "./entities";

export interface DispatcherCalloutDTOClientWrapper {
  Callout: {
    Id: string;
    Title: string;
    Description: string;
    Priority: FPriority;
    Location: Vector3;
    GeneratedAt: string;
    Units: string[];
  };
  Address: string;
}

export interface CalloutRemoveFromDispatcher {
  CalloutId: string;
}
