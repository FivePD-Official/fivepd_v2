import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CoordinateWithDate } from '@store/app.reducer';
import { PedHistory } from "@models";
import { Routes } from "../models/sidebar";

export const AppActions = createActionGroup({
  source: 'app',
  events: {
    'Set MDT path': props<{ path: string[] }>(),
    'Add dev tools coordinate': props<{ coordinate: CoordinateWithDate }>(),
    'Clear dev tools coordinates': emptyProps(),
    'Toggle dispatch mode': props<{ state: boolean }>(),
    'Dispatch mode map zoom': props<{ zoom: number }>(),
    'Set window maximized': props<{ maximized: boolean }>(),
    'Add ped to recently viewed': props<{ ped: PedHistory }>(),
  },
});
