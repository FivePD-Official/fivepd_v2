import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionResponse } from '@models';

export type DeleteEntityType = 'peds' | 'vehicles' | 'props' | 'all';

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
}
