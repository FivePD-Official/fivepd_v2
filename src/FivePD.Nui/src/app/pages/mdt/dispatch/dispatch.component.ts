import { Component, OnInit } from '@angular/core';
import {
  CRS,
  extend, icon,
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
import { Callout, DispatchService } from "@services/dispatch/dispatch.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { dispatchModeMapZoom } from "@store/app.actions";
import { DispatcherUnitDTO, Dropdown, DropdownItem, SimplePlayer } from "@models";

type MarkerIconProperties = {
  readonly size: [number, number];
}
type MarkerIconName = 'code3' | 'code2' | 'code1' | 'player';

const markerIcons: Record<MarkerIconName, MarkerIconProperties> = {
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
  }
};

@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.sass']
})
export class DispatchComponent implements OnInit {

  defaultZoom: number = 3;

  readonly minZoom = 2;
  readonly maxZoom = 5;

  readonly center_x = 117.3;
  readonly center_y = 172.8;
  readonly scale_x = 0.02072;
  readonly scale_y = 0.0205;
  readonly AtlasStyle = tileLayer('/assets/map/styles/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: this.minZoom,
    maxZoom: this.maxZoom,
    id: 'styleAtlas map',
    keepBuffer: 100,
    noWrap: true,
  });

  readonly markerLayerGroup = layerGroup();

  map?: LMap;
  customCrs: any;

  incomingCallouts: Callout[] = [];
  activeCallouts: Callout[] = [];
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
      const calloutsToRemove = this.incomingCallouts
        .filter(x => !callouts.some(y => y.Callout.Id === x.Callout.Id))
        .filter(x => !this.activeCallouts.some(y => y.Callout.Id === x.Callout.Id));
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

    this.dispatchService.activeCallouts$.pipe(takeUntilDestroyed()).subscribe(callouts => {
      this.activeCallouts = callouts;
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
    this.dispatchService.getIncomingCallouts();
    this.dispatchService.getUnits();

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

    this.initializeMap();
  }

  ngOnDestroy(): void {
  }

  customIcon(name: MarkerIconName) {
    const markerIcon = markerIcons[name];
    return LIcon({
      iconUrl: `/assets/map/icons/${name}.png`,
      iconSize: markerIcon.size,
      iconAnchor: markerIcon.size,
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
  }

  jumpToMarker(callout: DispatcherCalloutDTOClientWrapper): void {
    if (!this.map) {
      return;
    }

    try {
      this.map.setView([callout.Callout.Location.Y, callout.Callout.Location.X], 4);
    } catch (_) {
    }
  }

  dismissCallout(callout: DispatcherCalloutDTOClientWrapper): void {
    this.dispatchService.dismissCallout(callout).subscribe();
  }

  getAssignCalloutToUnitsField(callout: DispatcherCalloutDTOClientWrapper): Dropdown {
    const dropdown = new Dropdown({
      key: '#',
      dropdownOptions: {
        label: 'Assign units',
        items: this.units
          .filter(unit => !this.activeCallouts.some(c => c.units.some(u => u === unit.Callsign)))
          .map(unit => ({ key: unit.Callsign, label: unit.Callsign })),
        size: 'small',
        height: 'base',
        variant: 'tertiary-bg',
        selectedItemsResolver: () => false,
      },
      required: false,
    });

    return dropdown;
  }

  onAssignCalloutToUnitsFieldChange(unitCallsigns: string[], callout: Callout): void {
    this.dispatchService.assignCalloutToUnits(unitCallsigns, callout);
  }

  join(a: string[]): string {
    return a.join(', ');
  }

  private initializeMap(): void {
    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.map = map('map', {
      crs: this.customCrs,
      minZoom: this.minZoom,
      maxZoom: this.maxZoom,
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
  }

  private createCalloutMarker(callout: DispatcherCalloutDTOClientWrapper): void {
    const existingMarker = this.calloutMarkers.get(callout.Callout.Id);
    if (!existingMarker) {
      const {X, Y} = callout.Callout.Location;
      let icon: MarkerIconName;
      switch (callout.Callout.Priority) {
        case FIncidentPriority.Code1:
          icon = 'code1';
          break;
        case FIncidentPriority.Code2:
          icon = 'code2';
          break;
        case FIncidentPriority.Code3:
          icon = 'code3';
          break;
      }
      const marker = new Marker([Y, X])
        .addTo(this.markerLayerGroup)
        .setIcon(this.customIcon(icon));
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
  protected readonly FIncidentPriority = FIncidentPriority;
}
