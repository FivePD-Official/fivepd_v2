import { Injectable } from '@angular/core';
import { BaseApiService } from "@services/api/base-api/base-api.service";
import { Collation, Id, Page, SpeedZoneWithPlayerDTO } from "@models";
import { Observable, of } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class SceneManagementService extends BaseApiService<SpeedZoneWithPlayerDTO> {

  override baseUrl = 'fivepd:sceneManagement';

  get(id: Id) {
    return of();
  }

  getPage(collation: Collation) {
    return of();
  }

  getAll() {
    return this.httpClient.post<SpeedZoneWithPlayerDTO[]>(this.createUrl('getAllSpeedZones'), {});
  }

}
