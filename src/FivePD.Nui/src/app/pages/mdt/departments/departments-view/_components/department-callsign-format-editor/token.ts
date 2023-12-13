import { InjectionToken } from "@angular/core";
import { CallsignFormatItem } from "@models";

export const CALLSIGN_FORMAT_ITEMS = new InjectionToken<CallsignFormatItem[]>("Callsign format items");
