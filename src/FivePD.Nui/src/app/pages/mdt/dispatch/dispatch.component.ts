import { Component, OnInit } from '@angular/core';
import {
  CRS,
  extend,
  icon as LIcon,
  latLng,
  latLngBounds,
  layerGroup,
  map,
  Map as LMap,
  Marker,
  Projection,
  tileLayer,
  Transformation
} from 'leaflet';
import { DispatcherCalloutDTOClientWrapper } from "../../../models/callout";
import { FIncidentPriority, FUnitAvailability } from "@enums";
import { DispatchService } from "@services/dispatch/dispatch.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { dispatchModeMapZoom } from "@store/app.actions";
import { DispatcherUnitDTO, SimplePlayer } from "@models";

type IconType = 'callout' | 'player';

@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.sass']
})
export class DispatchComponent implements OnInit {

  defaultZoom: number = 3;

  readonly center_x = 117.3;
  readonly center_y = 172.8;
  readonly scale_x = 0.02072;
  readonly scale_y = 0.0205;
  readonly AtlasStyle = tileLayer('/assets/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: 2,
    maxZoom: 5,
    id: 'styleAtlas map',
    keepBuffer: 100,
    noWrap: true,
  });

  readonly markerLayerGroup = layerGroup();

  map?: LMap;
  CUSTOM_CRS: any;

  incomingCallouts: DispatcherCalloutDTOClientWrapper[] = [];
  calloutMarkers: Map<string, Marker> = new Map();

  units: DispatcherUnitDTO[] = [];
  players: SimplePlayer[] = [];
  playerMarkers: Map<number, Marker> = new Map();

  // layersControl = L.control.layers({ "Satelite": SateliteStyle,"Atlas": AtlasStyle,"Grid":GridStyle}).addTo(mymap);

  constructor(
    public readonly dispatchService: DispatchService,
    private readonly _store: Store<GlobalState>,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.defaultZoom = state.app.dispatchModeMapZoom;
    });

    this.dispatchService.incomingCallouts$.pipe(takeUntilDestroyed()).subscribe(callouts => {
      const calloutsToRemove = this.incomingCallouts.filter(x => !callouts.some(y => y.Callout.Id === x.Callout.Id));
      const calloutsToAdd = callouts.filter(x => !this.incomingCallouts.some(y => y.Callout.Id === x.Callout.Id));

      for (const callout of calloutsToRemove) {
        const marker = this.calloutMarkers.get(callout.Callout.Id);
        marker?.remove();
        this.calloutMarkers.delete(callout.Callout.Id);
      }

      for (const callout of calloutsToAdd) {
        this.createCalloutMarker(callout);
      }

      this.incomingCallouts = callouts;
    });

    this.dispatchService.units$.pipe(takeUntilDestroyed()).subscribe(units => {
      this.units = units;
    });

    this.dispatchService.players$.pipe(takeUntilDestroyed()).subscribe(players => {
      const playersToRemove = this.players.filter(x => !players.some(y => y.NetworkId === x.NetworkId));
      for (const player of playersToRemove) {
        const marker = this.playerMarkers.get(player.NetworkId);
        marker?.remove();
        this.playerMarkers.delete(player.NetworkId);
      }

      for (const player of players) {
        this.createOrUpdatePlayerMarker(player);
      }

      this.players = players;
    });
  }

  ngOnInit(): void {
    this.dispatchService.getActiveCallouts();
    this.dispatchService.getUnits();

    this.CUSTOM_CRS = extend({}, CRS.Simple, {
      projection: Projection.LonLat,
      scale: function (zoom: any) {
        return Math.pow(2, zoom);
      },
      zoom: function (sc: any) {
        return Math.log(sc) / 0.6931471805599453;
      },
      distance: function (pos1: any, pos2: any) {
        var x_difference = pos2.lng - pos1.lng;
        var y_difference = pos2.lat - pos1.lat;
        return Math.sqrt(x_difference * x_difference + y_difference * y_difference);
      },
      transformation: new Transformation(this.scale_x, this.center_x, -this.scale_y, this.center_y),
      infinite: true,
    });

    this.initializeMap();
  }

  ngOnDestroy(): void {
  }

  customIcon(type: IconType) {
    return LIcon({
      iconUrl: `/assets/map/icons/${type}.png`,
      iconSize: [23, 28],
      iconAnchor: [23, 28],
      popupAnchor: [-10, -27],
    });
  }

  getPriorityLabel(callout: DispatcherCalloutDTOClientWrapper): string {
    switch (callout.Callout.Priority) {
      case FIncidentPriority.Code1:
        return 'LABEL.PRIORITY.CODE1';
      case FIncidentPriority.Code2:
        return 'LABEL.PRIORITY.CODE2';
      case FIncidentPriority.Code3:
        return 'LABEL.PRIORITY.CODE3';
    }

    return '-';
  }

  jumpToMarker(callout: DispatcherCalloutDTOClientWrapper): void {
    if (!this.map) {
      return;
    }

    try {
      this.map.setView([callout.Callout.Location.Y, callout.Callout.Location.X], 3);
    } catch (_) {
    }
  }

  dismissCallout(callout: DispatcherCalloutDTOClientWrapper): void {
    this.dispatchService.dismissCallout(callout).subscribe();
  }

  private initializeMap(): void {
    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.map = map('map', {
      crs: this.CUSTOM_CRS,
      minZoom: 2,
      maxZoom: 5,
      preferCanvas: false,
      layers: [this.AtlasStyle, this.markerLayerGroup],
      center: [0, 0],
      zoom: this.defaultZoom,
      zoomControl: false,
    });
    this.map.setMaxBounds(atlasBounds);

    this.map.on('drag', () => {
      this.map?.panInsideBounds(atlasBounds, {animate: false});
    });

    this.map.on('zoom', e => {
      this._store.dispatch(dispatchModeMapZoom({payload: e.target['_zoom']}));
    });

    // this.map.on('click', e => {
    //   if (this.marker) { // check
    //     this.map?.removeLayer(this.marker);
    //   }
    //   const {lat, lng} = e.latlng;
    //   const zoom = 3;
    //   this.marker = new Marker([lat, lng]).addTo(this.Icons["Example"]).bindPopup("<b>X: " + lng.toFixed(3) + " | Y: " + lat.toFixed(3) + "</b>");
    //   this.map?.setView([lat, lng], zoom);
    // });
  }

  private createCalloutMarker(callout: DispatcherCalloutDTOClientWrapper): void {
    const existingMarker = this.calloutMarkers.get(callout.Callout.Id);
    if (!existingMarker) {
      const {X, Y} = callout.Callout.Location;
      const marker = new Marker([Y, X])
        .addTo(this.markerLayerGroup)
        .bindPopup("<b>X: " + X + " | Y: " + Y + "</b>")
        .setIcon(this.customIcon('callout'));
      this.calloutMarkers.set(callout.Callout.Id, marker);
    }
  }

  private createOrUpdatePlayerMarker(player: SimplePlayer): void {
    const marker = this.playerMarkers.get(player.NetworkId);
    if (marker && this.map?.hasLayer(marker)) {
      marker.setLatLng([player.Position.Y, player.Position.X]);
    } else {
      const marker = new Marker([player.Position.Y, player.Position.X])
        .addTo(this.markerLayerGroup)
        .setIcon(this.customIcon('player'));
      this.playerMarkers.set(player.NetworkId, marker);
    }
  }

  protected readonly FUnitAvailability = FUnitAvailability;
}
