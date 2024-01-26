import { divIcon, LatLng, Map as LMap, Marker, PathOptions, PointExpression, Polygon } from "leaflet";
import { DepartmentAreaCoordinate, SimpleDepartmentWithArea } from "@models";

export const GRAY = '#52525b';
export const RED = '#dc2626';

export class Area {

  readonly polygon!: Polygon;
  readonly coordinates = new Map<number, LatLng>();
  private polygonLabel?: Marker;

  beingEdited = false;

  constructor(private readonly department: SimpleDepartmentWithArea,
              private readonly map: LMap,
              private readonly areaCoordinates: DepartmentAreaCoordinate[] = []) {
    this.coordinates = new Map(areaCoordinates.map(x => ([x.Id, new LatLng(x.Y, x.X)])));
    this.polygon = new Polygon(areaCoordinates.map(x => ([x.Y, x.X])), {
      fillColor: GRAY,
      fillOpacity: 0.35,
      color: GRAY,
    })
      .addTo(map)
      .on('mouseover', () => {
        this.polygon.setStyle({
          fillColor: RED,
          fillOpacity: 0.3,
          color: RED,
        });
      })
      .on('mouseout', () => {
        const style: PathOptions = {
          fillColor: GRAY,
          fillOpacity: 0.35,
        };

        if (!this.beingEdited) {
          style.color = GRAY;
        }

        this.polygon.setStyle(style);
      });
  }

  redraw(): void {
    this.polygon.redraw();
    this.drawPolygonLabel();
  }

  destroy(): void {
    this.polygon.remove();
    this.polygonLabel?.remove();
  }

  private calculateIconSize(content: string): PointExpression {
    const div = document.createElement('div');
    div.style.width = 'max-content'
    div.innerHTML = content;
    document.body.appendChild(div);

    const width = div.clientWidth;
    const height = div.clientHeight;

    document.body.removeChild(div);

    return [width, height];
  }

  private drawPolygonLabel(): void {
    if (this.polygonLabel) {
      this.polygonLabel.remove();
    }

    const bounds = this.polygon.getBounds();
    if (!bounds.isValid()) {
      return;
    }

    const center = bounds.getCenter();
    this.polygonLabel = new Marker(center, {
      icon: divIcon({
        className: 'leaflet-div-label important-pointer-events-none',
        html: `<div>
                <p class="text-zinc-900 font-extrabold text-center font-sans"
                   style="font-size: 22px !important;">
                    ${this.department.Callsign}
                </p>
            </div>`,
        iconSize: this.calculateIconSize(this.department.Name),
      }),
    }).addTo(this.map);
  }
}
