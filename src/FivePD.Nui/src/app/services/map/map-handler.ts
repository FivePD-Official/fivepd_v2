import {
  CRS,
  extend, icon as LIcon,
  latLng,
  LatLng,
  latLngBounds,
  layerGroup,
  map,
  Map as LMap,
  Marker,
  Projection,
  tileLayer,
  Transformation
} from 'leaflet';
import { MarkerIconName, markerIcons } from "@services/map/index";

export class MapHandler {

  private readonly defaultZoom: number = 2;

  private readonly center_x = 117.3;
  private readonly center_y = 172.8;
  private readonly scale_x = 0.02072;
  private readonly scale_y = 0.0205;
  private readonly AtlasStyle = tileLayer('/assets/map/styles/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: this.defaultZoom,
    maxZoom: 5,
    id: 'styleAtlas map',
    keepBuffer: 100,
    noWrap: true,
  });

  private readonly markerLayerGroup = layerGroup();

  readonly map!: LMap;
  private readonly customCrs: any;

  constructor() {
    this.customCrs = extend({}, CRS.Simple, {
      projection: Projection.LonLat,
      scale: function (zoom: any) {
        return Math.pow(2, zoom);
      },
      zoom: function (sc: any) {
        return Math.log(sc) / 0.6931471805599453;
      },
      distance: function (pos1: any, pos2: any) {
        const x_difference = pos2.lng - pos1.lng;
        const y_difference = pos2.lat - pos1.lat;
        return Math.sqrt(x_difference * x_difference + y_difference * y_difference);
      },
      transformation: new Transformation(this.scale_x, this.center_x, -this.scale_y, this.center_y),
      infinite: true,
    });

    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.map = map('map', {
      crs: this.customCrs,
      minZoom: this.defaultZoom,
      maxZoom: this.defaultZoom,
      preferCanvas: false,
      layers: [this.AtlasStyle, this.markerLayerGroup],
      center: [0, 0],
      zoom: this.defaultZoom,
      zoomControl: false,
      dragging: false,
    });

    this.map.setMaxBounds(atlasBounds);
  }

  addMarker<T>(options: {latLng: LatLng, attribution?: T}): Marker {
    return new Marker(options.latLng, {
      draggable: true,
      attribution: options.attribution as any,
    }).addTo(this.map);
  }

  createIcon(name: MarkerIconName) {
    const markerIcon = markerIcons[name];
    return LIcon({
      iconUrl: `/assets/map/icons/${name}.png`,
      iconSize: markerIcon.size,
      iconAnchor: markerIcon.size,
      popupAnchor: [-10, -27],
    });
  }
}
