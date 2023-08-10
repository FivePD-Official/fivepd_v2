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
import { EventService } from "@services/event/event.service";
import { CalloutRemoveFromDispatcher, DispatcherCalloutDTOClientWrapper } from "../../../models/callout";
import { FIncidentPriority, NuiEventType } from "@enums";
import { ConfigService } from "@services/config/config.service";
import { GeneralConfig } from "@models";
import { DispatchService } from "@services/dispatch/dispatch.service";

type Callout = DispatcherCalloutDTOClientWrapper & {
  timeout: number;
  generatedAt: Date;
}

type IconType = 'callout' | 'unit';

@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.sass']
})
export class DispatchComponent implements OnInit {

  readonly defaultZoom = 3;

  readonly center_x = 117.3;
  readonly center_y = 172.8;
  readonly scale_x = 0.02072;
  readonly scale_y = 0.0205;
  readonly AtlasStyle = tileLayer('/assets/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: 2,
    maxZoom: 5,
    id: 'styleAtlas map',
    keepBuffer: 100,
  });

  readonly ExampleGroup = layerGroup();

  readonly Icons = {
    "Example": this.ExampleGroup,
  };

  generalConfig?: GeneralConfig;

  mymap!: LMap;
  CUSTOM_CRS: any;
  marker: any;

  calloutMarkers: Map<string, Marker> = new Map();
  maxTimeout: number = 0;
  timeoutIntervalId: any;

  // layersControl = L.control.layers({ "Satelite": SateliteStyle,"Atlas": AtlasStyle,"Grid":GridStyle}).addTo(mymap);

  incomingCallouts: Callout[] = [];

  constructor(
    private readonly _eventService: EventService,
    private readonly _configService: ConfigService,
    private readonly _dispatchService: DispatchService,
  ) {
    _configService.getGeneralConfig().subscribe(config => {
      this.generalConfig = config;
      this.maxTimeout = Math.round(config.config.Callout.DispatchTimeoutInMs / 1000);
    });

    _eventService
      .onEvent<DispatcherCalloutDTOClientWrapper>(NuiEventType.SendCalloutToDispatcher)
      .subscribe(callout => this.onCalloutReceived(callout));

    _eventService
      .onEvent<CalloutRemoveFromDispatcher>(NuiEventType.RemoveCalloutFromDispatcher)
      .subscribe(event => this.onCalloutRemove(event.CalloutId));
  }

  ngOnInit(): void {
    this._dispatchService.getActiveCallouts().subscribe(callouts => {
      this.incomingCallouts = callouts.map(c => this.mapCallout(c));
    });

    this.timeoutIntervalId = setInterval(() => {
      const now = new Date();
      for (const callout of this.incomingCallouts) {
        callout.timeout = this.maxTimeout - Math.round(this.calculateSecondsDifference(callout.generatedAt, now));
      }
    }, 1000);

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
    clearInterval(this.timeoutIntervalId);
    this.mymap.remove();
    this.incomingCallouts = [];
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

  jumpToCallout(callout: DispatcherCalloutDTOClientWrapper): void {
    if (!this.mymap) {
      return;
    }

    try {
      this.mymap.setView([callout.Callout.Location.Y, callout.Callout.Location.X], this.defaultZoom);
    } catch (_) {
    }
  }

  dismissCallout(callout: DispatcherCalloutDTOClientWrapper): void {
    this._dispatchService.dismissCallout(callout).subscribe();
  }

  private initializeMap(): void {
    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.mymap = map('map', {
      crs: this.CUSTOM_CRS,
      minZoom: 2,
      maxZoom: 5,
      preferCanvas: false,
      layers: [this.AtlasStyle, this.Icons["Example"]],
      center: [0, 0],
      zoom: this.defaultZoom,
      zoomControl: false,
    });
    this.mymap.setMaxBounds(atlasBounds);

    this.mymap.on('drag', () => {
      this.mymap.panInsideBounds(atlasBounds, {animate: false});
    });

    this.mymap.on('click', e => {
      if (this.marker) { // check
        this.mymap.removeLayer(this.marker);
      }
      const {lat, lng} = e.latlng;
      const zoom = 3;
      this.marker = new Marker([lat, lng]).addTo(this.Icons["Example"]).bindPopup("<b>X: " + lng.toFixed(3) + " | Y: " + lat.toFixed(3) + "</b>");
      this.mymap.setView([lat, lng], zoom);
    });
  }

  private onCalloutReceived(callout: DispatcherCalloutDTOClientWrapper): void {
    this.incomingCallouts.push(this.mapCallout(callout));
    // this.mymap.eachLayer(layer => {
    // if(layer['_latlng']) {
    //   layer.remove();
    // }
    // });

    const {X, Y} = callout.Callout.Location;
    const marker = new Marker([Y, X])
      .addTo(this.Icons["Example"])
      .bindPopup("<b>X: " + X + " | Y: " + Y + "</b>")
      .setIcon(this.customIcon('callout'));
    this.calloutMarkers.set(callout.Callout.Id, marker);
    this.jumpToCallout(callout);
    // $("#markerSkripta").empty().append("<script>"+azurirajmarker+"</script>");
  }

  private onCalloutRemove(calloutId: string): void {
    this.calloutMarkers.get(calloutId)?.remove();
    this.calloutMarkers.delete(calloutId);
    this.incomingCallouts = this.incomingCallouts.filter(x => x.Callout.Id !== calloutId);
  }

  private mapCallout(callout: DispatcherCalloutDTOClientWrapper): Callout {
    return {
      ...callout,
      timeout: 0,
      generatedAt: new Date(callout.Callout.GeneratedAt)
    };
  }

  private calculateSecondsDifference(date1: Date, date2: Date): number {
    const millisecondsDifference = Math.abs((date2 as any) - (date1 as any));
    return millisecondsDifference / 1000;
  }
}
