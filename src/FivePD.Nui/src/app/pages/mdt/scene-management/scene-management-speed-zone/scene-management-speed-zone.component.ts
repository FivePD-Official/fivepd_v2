import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SceneManagementService } from "@services/scene-management/scene-management.service";
import { BehaviorSubject, map, switchMap } from "rxjs";
import { GridComponent } from "../../../../components/grid/grid.component";
import { Column, Page, SpeedZoneWithPlayerDTO } from "@models";
import { UserRole, UserService } from "@services/user/user.service";

@Component({
  selector: 'app-scene-management-speed-zone',
  standalone: true,
  imports: [CommonModule, GridComponent],
  templateUrl: './scene-management-speed-zone.component.html',
})
export class SceneManagementSpeedZoneComponent {

  protected readonly sceneManagementService = inject(SceneManagementService);
  protected readonly _userService = inject(UserService);

  readonly refreshSpeedZones$ = new BehaviorSubject<null>(null);
  readonly speedZones$ = this.refreshSpeedZones$.pipe(
    switchMap(() => this.sceneManagementService.getAll()),
    map(speedZone => {
      const page: Page<SpeedZoneWithPlayerDTO> = {
        PageSize: speedZone.length,
        PageCount: speedZone.length,
        Items: speedZone,
        CurrentPage: 1,
        ItemCount: speedZone.length,
      }
      return page;
    }),
  );

  readonly columns: Column<SpeedZoneWithPlayerDTO>[] = [
    new Column({
      header: 'Created by',
      valueResolver: x => x.PlayerName ?? 'Addon',
    }),
    new Column({
      header: 'Speed',
      valueResolver: x => x.SpeedZone.Speed.toString(),
    }),
    new Column({
      header: 'Radius',
      valueResolver: x => x.SpeedZone.Radius.toString(),
    }),
    new Column({
      header: 'Position',
      valueResolver: x => `${x.SpeedZone.Position.X}, ${x.SpeedZone.Position.Y}, ${x.SpeedZone.Position.Z}`,
    }),
    new Column({
      header: 'Actions',
      show: () => this._userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: 'Delete',
          onClick: speedZone => {
            this.sceneManagementService.delete(speedZone.Id).subscribe(() => this.refreshSpeedZones$.next(null));
          },
        }
      ]
    }),
  ];

  // private mapHandler!: MapHandler;

  // ngAfterViewInit(): void {
  //   this.mapHandler = new MapHandler();
  // }

}
