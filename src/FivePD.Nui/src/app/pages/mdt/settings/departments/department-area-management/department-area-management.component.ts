import {Component, inject, OnInit} from '@angular/core';
import {SimpleDepartment, SimpleDepartmentWithArea, User} from "@models";
import {
  CRS,
  extend,
  latLng,
  LatLng,
  latLngBounds,
  layerGroup,
  LeafletMouseEvent,
  map,
  Map as LMap,
  Marker,
  Projection,
  tileLayer,
  Transformation
} from 'leaflet';
import {PageHeaderComponent} from "../../../../../components/page-header/page-header.component";
import {TranslatePipe} from "../../../../../pipes/translate.pipe";
import {DepartmentService} from "@services/department/department.service";
import {ContainerComponent} from "../../../../../components/container/container.component";
import {nanoid} from 'nanoid';
import {ButtonComponent} from "../../../../../components/button/button.component";
import {NgClass} from "@angular/common";
import {Area, GRAY, RED} from "./_models/area";
import {customIcon} from "@services/map";
import {IconComponent} from "../../../../../components/icon/icon.component";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {first} from "rxjs";
import {Store} from "@ngrx/store";
import {GlobalState} from "@store";
import {UserRole, UserService} from "@services/user/user.service";

// TODO: lng = X, lat = Y

type MarkerAttribution = {
  id: string;
  departmentId: number | string;
}

@Component({
  selector: 'app-department-area-management',
  standalone: true,
  imports: [PageHeaderComponent, TranslatePipe, ContainerComponent, ButtonComponent, NgClass, IconComponent],
  templateUrl: './department-area-management.component.html',
})
export class DepartmentAreaManagementComponent implements OnInit {

  private readonly departmentService = inject(DepartmentService);

  departments: SimpleDepartmentWithArea[] = [];
  currentlyEditingDepartment?: SimpleDepartmentWithArea;
  areas: Record<number | string, Area> = {};

  markers: Marker[] = [];
  canAddPolygonCoordinate = false;

  modified = false;

  user?: User;

  constructor(
    public readonly userService: UserService,
    private readonly _store: Store<GlobalState>,
  ) {
    _store.pipe(takeUntilDestroyed(), first()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }

  get editableDepartments() {
    if (this.userService.userHasRole(UserRole.Admin)) {
      return this.departments;
    }

    return this.departments.filter(x => x.Id === this.user?.DepartmentId);
  }

  readonly defaultZoom: number = 2;

  readonly center_x = 117.3;
  readonly center_y = 172.8;
  readonly scale_x = 0.02072;
  readonly scale_y = 0.0205;
  readonly AtlasStyle = tileLayer('/assets/map/styles/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: this.defaultZoom,
    maxZoom: 5,
    id: 'styleAtlas map',
    keepBuffer: 100,
    noWrap: true,
  });

  readonly markerLayerGroup = layerGroup();

  map!: LMap;
  customCrs: any;


  ngOnInit(): void {
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
    this.getDepartments();
  }

  departmentHasEnoughAreaCoordinates(department: SimpleDepartmentWithArea): boolean {
    const area = this.areas[department.Id];
    if (!area) {
      return false;
    }

    return area.coordinates.size >= 3;
  }

  toggleEdit(department?: SimpleDepartmentWithArea): void {
    for (const marker of this.markers) {
      marker.remove();
    }
    this.markers = [];
    this.canAddPolygonCoordinate = false;

    if (this.currentlyEditingDepartment) {
      const area = this.areas[this.currentlyEditingDepartment.Id];
      if (area) {
        area.beingEdited = false;
        area.polygon.setStyle({
          color: GRAY,
        });
      }
    }

    if (!department) {
      this.currentlyEditingDepartment = undefined;
      return;
    }

    if (this.currentlyEditingDepartment?.Id === department.Id) {
      this.currentlyEditingDepartment = undefined;
      return;
    }

    this.currentlyEditingDepartment = department;
    const area = this.areas[department.Id];
    if (!area) {
      return;
    }

    area.beingEdited = true;
    area.polygon.setStyle({
      color: RED,
    });

    for (const [id, latLng] of area.coordinates.entries()) {
      const marker = this.addMarker(id, latLng, department);
      this.markers.push(marker);
      marker.dragging?.enable();
    }
  }

  toggleCoordinateAdd(): void {
    if (!this.currentlyEditingDepartment) {
      return;
    }

    this.canAddPolygonCoordinate = !this.canAddPolygonCoordinate;
    if (this.canAddPolygonCoordinate) {
      for (const marker of this.markers) {
        marker.dragging?.disable();
      }
    } else {
      for (const marker of this.markers) {
        marker.remove();
      }
      this.markers = [];
      const area = this.areas[this.currentlyEditingDepartment.Id];
      if (!area) {
        return;
      }

      for (const [id, latLng] of area.coordinates.entries()) {
        const marker = this.addMarker(id, latLng, this.currentlyEditingDepartment);
        this.markers.push(marker);
        marker.dragging?.enable();
      }
    }
  }

  onSave(): void {
    const request = {
      Areas: Object.entries(this.areas).map((([departmentId, area]) => ({
        DepartmentId: departmentId,
        Coordinates: Array.from(area.coordinates.entries()).map(([id, latLng]) => ({
          Id: typeof id === 'string' ? -1 : id,
          X: latLng.lng,
          Y: latLng.lat,
        })),
      }))),
    };

    this.departmentService.updateAreas(request).subscribe(() => {
      this.modified = false;
      this.toggleEdit(undefined);
      this.getDepartments();
    });
  }

  private addNewCoordinate(e: LeafletMouseEvent): void {
    if (!this.currentlyEditingDepartment || !this.canAddPolygonCoordinate) {
      return;
    }

    const area = this.areas[this.currentlyEditingDepartment.Id];
    if (!area) {
      return;
    }

    const markerId = nanoid() as any;
    area.coordinates.set(markerId, e.latlng);

    const marker = this.addMarker(markerId, e.latlng, this.currentlyEditingDepartment);
    this.markers.push(marker);

    area.polygon.addLatLng(e.latlng);
    area.redraw();
    this.modified = true;
  }

  private addMarker(id: number, latLng: LatLng, department: SimpleDepartment): Marker {
    const marker = new Marker(latLng, {
      draggable: true,
      attribution: {
        id,
        departmentId: department.Id,
      } as any,
    })
      .setIcon(customIcon('departmentArea'))
      .addTo(this.map)
      .on('contextmenu', e => {
        const area = this.areas[department.Id];
        if (!area) {
          return;
        }

        area.coordinates.delete(id);
        marker.remove();

        area.polygon.setLatLngs(Array.from(area.coordinates.values()));
        area.redraw();
        this.modified = true;
      })
      .on('drag', e => {
        const departmentId = marker.getAttribution?.() as unknown as MarkerAttribution;
        const area = this.areas[departmentId.departmentId];
        if (!area) {
          return;
        }

        area.coordinates.set(id, e.target.getLatLng());

        area.polygon.setLatLngs(Array.from(area.coordinates.values()));
        area.redraw();
        this.modified = true;
      });

    marker.dragging?.disable();

    return marker;
  }

  private initializeMap(): LMap {
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
    this.map.on('click', e => this.addNewCoordinate(e));

    return this.map;
  }

  private getDepartments(): void {
    this.departmentService.getAllSimpleWithArea().subscribe(departments => {
      this.departments = departments;

      for (const area of Object.values(this.areas)) {
        area.destroy();
      }
      this.areas = {};

      for (const department of departments) {
        const area = new Area(department, this.map, department.Coordinates);
        this.areas[department.Id] = area;
        area.redraw();
      }
    });
  }

  protected readonly UserRole = UserRole;
}
