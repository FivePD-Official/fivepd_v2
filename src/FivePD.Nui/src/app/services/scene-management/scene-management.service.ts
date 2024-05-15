import { Injectable } from '@angular/core';
import { BaseApiService } from "@services/api/base-api/base-api.service";
import { ActionResponse, Collation, Id, Page, SpeedZoneWithPlayerDTO } from "@models";
import { Observable, of } from "rxjs";
import { parseId } from "../../helpers/parse-id";

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

  delete(id: Id) {
    return this.httpClient.post<ActionResponse>(this.createUrl('deleteUI'), {
      Id: id,
    });
  }

}
