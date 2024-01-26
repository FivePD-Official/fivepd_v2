import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Routes, RouteWithChildren } from "../models/sidebar";

export const SidebarActions = createActionGroup({
  source: 'sidebar',
  events: {
    'Set routes': props<{ routes: Routes }>(),
    'Toggle route': props<{ route: RouteWithChildren }>(),
    'Toggle collapse': emptyProps(),
  },
});
