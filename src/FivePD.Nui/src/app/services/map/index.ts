import { icon as LIcon } from "leaflet";

export * from './map-handler';

export type MarkerIconProperties = {
  readonly size: [number, number];
}
export type MarkerIconName =
  | 'code3'
  | 'code2'
  | 'code1'
  | 'player'
  | 'departmentArea'
  | 'speedZone';

export const markerIcons: Record<MarkerIconName, MarkerIconProperties> = {
  code3: {
    size: [33, 38],
  },
  code2: {
    size: [33, 38],
  },
  code1: {
    size: [33, 38],
  },
  player: {
    size: [38, 38],
  },
  departmentArea: {
    size: [33, 38],
  },
  speedZone: {
    size: [33, 38],
  },
};

export function customIcon(name: MarkerIconName) {
  const markerIcon = markerIcons[name];
  return LIcon({
    iconUrl: `/assets/map/icons/${name}.png`,
    iconSize: markerIcon.size,
    iconAnchor: [markerIcon.size[0] / 2, markerIcon.size[1] / 2 + 10],
  });
}
