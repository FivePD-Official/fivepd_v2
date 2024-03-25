import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionResponse, Collation, Id, Location, LocationType, Page } from '@models';
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class LocationService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  getAllPageable(type: LocationType, collation?: Collation): Observable<Page<Location>> {
    return this._http.post<Page<Location>>('fivepd:ui:location:getAllPageable', {
      ...collation,
      Filters: {
        Type: type,
        ...collation?.Filters,
      },
    });
  }

  create(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:location:create', request);
  }

  delete(id: Id): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:location:delete', {Id: parseId(id)});
  }
}
