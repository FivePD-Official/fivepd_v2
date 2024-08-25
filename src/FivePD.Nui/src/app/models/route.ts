import { IconType } from "./icon";

export interface RouteWithChildren extends RouteWithOnClick {
  title: string;
  path?: never;
  icon: IconType;
  show: () => boolean;
  children?: RouteWithoutChildren[];
  open?: boolean;
}

export interface RouteWithoutChildren extends RouteWithOnClick {
  title: string;
  path: () => string;
  icon: IconType;
  show: () => boolean;
  children?: never;
}

export interface RouteWithOnClick {
  onClick?: () => void;
  linkActive?: () => boolean;
}

export type RouteOrDivider = RouteWithChildren | RouteWithoutChildren | { divider: true; };

export type Routes = RouteOrDivider[];
