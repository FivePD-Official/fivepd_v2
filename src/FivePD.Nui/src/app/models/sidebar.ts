import { IconType } from "./icon";

export interface RouteWithChildren {
  title: string;
  path?: never;
  icon: IconType;
  show: () => boolean;
  children?: RouteWithoutChildren[];
  open?: boolean;
}

export interface RouteWithoutChildren {
  title: string;
  path: () => string;
  icon: IconType;
  show: () => boolean;
  children?: never;
}


export type RouteOrDivider = RouteWithChildren | RouteWithoutChildren | { divider: true; };

export type Routes = RouteOrDivider[];
