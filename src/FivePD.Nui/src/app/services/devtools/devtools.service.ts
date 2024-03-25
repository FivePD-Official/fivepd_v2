import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionResponse, DeleteEntityType, Vector3 } from '@models';

@Injectable({
  providedIn: 'root'
})
export class DevtoolsService {

  constructor(
    private readonly http: HttpClient,
  ) { }

  deleteEntities(type: DeleteEntityType): Observable<ActionResponse> {
    return this.http.post<ActionResponse>('fivepd:devtools:deleteEntities', {
      Type: type,
    });
  }

  getCurrentPlayerPosition(): Observable<Vector3> {
    return this.http.post<Vector3>('fivepd:devtools:getCurrentPlayerCoordinate', {});
  }
}
