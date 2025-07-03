import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import {
  CRS,
  extend,
  icon as LIcon,
  latLng,
  latLngBounds,
  layerGroup,
  map as createMap,
  Map as LMap,
  Marker,
  Projection,
  tileLayer,
  Transformation
} from 'leaflet';
import { DispatcherCalloutDTOClientWrapper } from "../../../models/callout";
import { FPriority, FUnitAvailability } from "@enums";
import { Callout, DispatchService } from "@services/dispatch/dispatch.service";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { MultiSelectField, SimplePlayer } from "@models";
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { ButtonComponent } from '../../../components/button/button.component';
import { AsyncPipe, NgClass } from '@angular/common';
import { ContainerComponent } from '../../../components/container/container.component';
import { AppActions } from "@store/app.actions";
import {
  MultiSelectFieldComponent
} from "../../../components/form-inputs/multi-select-field/multi-select-field.component";
import { getPriorityLabel } from "../../../helpers/fpriority-helpers";
import { ConfigService } from "@services/config/config.service";
import { combineLatest, map, takeUntil } from "rxjs";
import { injectDestroy } from "../../../helpers/destroy-ref";

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
    styleUrls: ['./dispatch.component.sass'],
    imports: [ContainerComponent, NgClass, ButtonComponent, TranslatePipe, MultiSelectFieldComponent, AsyncPipe]
})
export class DispatchComponent implements OnInit, OnDestroy {

  readonly destroy$ = injectDestroy();
  readonly store = inject<Store<GlobalState>>(Store);
  readonly dispatchService = inject(DispatchService);
  readonly configService = inject(ConfigService);

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

  calloutMarkers: Map<string, Marker> = new Map();

  players: SimplePlayer[] = [];
  playerMarkers: Map<number, Marker> = new Map();

  // layersControl = L.control.layers({ "Satelite": SateliteStyle,"Atlas": AtlasStyle,"Grid":GridStyle}).addTo(mymap);

  autoRemoveEnabled = false;

  ngOnInit(): void {
    this.configService.getGeneralConfig().subscribe(config => {
      this.autoRemoveEnabled = config.config.Callout.AutoRemove.Enabled;
    });

    this.store.pipe(takeUntil(this.destroy$)).subscribe(state => {
      this.defaultZoom = state.app.dispatchModeMapZoom;
    });

    this.dispatchService.getUnits();

    combineLatest([
      this.dispatchService.incomingCallouts$,
      this.dispatchService.activeCallouts$
    ]).pipe(
      takeUntil(this.destroy$),
      map(([incoming, active]) => ([...incoming, ...active])),
    ).subscribe(callouts => {
      const calloutMarkerIds = [...this.calloutMarkers.keys()];
      const notExistingCalloutIds = calloutMarkerIds.filter(x => callouts.every(y => x !== y.Callout.Id));
      const newCallouts = callouts.filter(x => calloutMarkerIds.every(y => y !== x.Callout.Id));

      for (const id of notExistingCalloutIds) {
        const marker = this.calloutMarkers.get(id);
        marker?.remove();
        this.calloutMarkers.delete(id);
      }

      for (const callout of newCallouts) {
        this.createCalloutMarker(callout);
      }
    });

    this.dispatchService.players$.pipe(takeUntil(this.destroy$)).subscribe(players => {
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

    this.dispatchService.getIncomingCallouts();
    this.dispatchService.getActiveCallouts();

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
    return getPriorityLabel(callout.Callout.Priority);
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

  getAssignUnitToCalloutField(): MultiSelectField {
    return new MultiSelectField({
      key: '#',
      defaultItemLabel: 'Assign units',
      items: () => this.dispatchService.units$.value
        .filter(unit => !this.dispatchService.activeCallouts$.value.some(c => c.Callout.Units.some(u => u === unit.Callsign)))
        .map(unit => ({key: unit.Callsign, label: unit.Callsign})),
      size: 'small',
      height: 'base',
      variant: 'tertiary-bg',
      required: false,
    });
  }

  getUnassignUnitFromCalloutField(callout: Callout): MultiSelectField {
    return new MultiSelectField({
      key: '#',
      defaultItemLabel: 'Unassign units',
      items: () => callout.Callout.Units.map(unit => ({key: unit, label: unit})),
      size: 'small',
      height: 'base',
      variant: 'tertiary-bg',
      required: false,
    });
  }

  onAssignCalloutToUnitsFieldChange(unitCallsigns: any[], callout: Callout): void {
    this.dispatchService.assignCalloutToUnits(unitCallsigns, callout).subscribe();
  }

  onUnassignCalloutToUnitsFieldChange(unitCallsigns: any[], callout: Callout): void {
    this.dispatchService.unassignCalloutFromUnits(unitCallsigns, callout).subscribe();
  }

  join(a: string[]): string {
    return a.join(', ');
  }

  private initializeMap(): void {
    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.map = createMap('map', {
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
      this.store.dispatch(AppActions.dispatchModeMapZoom({zoom: e.target['_zoom']}));
    });
  }

  private createCalloutMarker(callout: DispatcherCalloutDTOClientWrapper): void {
    const existingMarker = this.calloutMarkers.get(callout.Callout.Id);
    if (!existingMarker) {
      const {X, Y} = callout.Callout.Location;
      let icon!: MarkerIconName;
      switch (callout.Callout.Priority) {
        case FPriority.Code1:
          icon = 'code1';
          break;
        case FPriority.Code2:
          icon = 'code2';
          break;
        case FPriority.Code3:
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
  protected readonly FPriority = FPriority;
}
