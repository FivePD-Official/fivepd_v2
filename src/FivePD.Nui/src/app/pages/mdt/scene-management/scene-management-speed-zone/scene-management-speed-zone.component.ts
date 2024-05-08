import { AfterViewInit, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapHandler } from "@services/map";
import { SceneManagementService } from "@services/scene-management/scene-management.service";
import { takeUntil, tap } from "rxjs";
import { LatLng } from "leaflet";
import { injectDestroy } from "../../../../helpers/destroy-ref";

@Component({
  selector: 'app-scene-management-speed-zone',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scene-management-speed-zone.component.html',
})
export class SceneManagementSpeedZoneComponent implements OnInit, AfterViewInit {

  private readonly destroy$ = injectDestroy();
  private readonly _sceneManagementService = inject(SceneManagementService);

  private mapHandler!: MapHandler;

  ngOnInit(): void {
    this._sceneManagementService.getAll()
      .pipe(
        takeUntil(this.destroy$),
        tap(speedZones => {
          console.log(speedZones)
          for (const zone of speedZones) {
            this.mapHandler.addMarker({
              latLng: new LatLng(zone.SpeedZone.Position.Y, zone.SpeedZone.Position.X),
            }).setIcon(this.mapHandler.createIcon('speedZone'))
          }
        }),
      )
      .subscribe();
  }

  ngAfterViewInit(): void {
    this.mapHandler = new MapHandler();
  }

}
