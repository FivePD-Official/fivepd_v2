import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {
  ActionResponse,
  Collation,
  Department,
  Division,
  NameTakenResponse,
  Page,
  Rank,
  SimpleDepartment, SimpleDepartmentWithArea,
  User
} from '@models';

@Injectable({
  providedIn: 'root'
})
export class VehicleConfigService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  add(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:ui:vehicleConfig:add', request);
  }
}
