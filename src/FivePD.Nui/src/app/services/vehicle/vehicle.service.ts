import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, FVehicle, Id, Page } from "@models";
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class VehicleService {

  constructor(private readonly _http: HttpClient) {
  }

  get(id: Id): Observable<FVehicle> {
    return this._http.post<FVehicle>('fivepd:vehicle:get', {
      Id: parseId(id),
    });
  }

  getAll(collation?: Collation): Observable<Page<FVehicle>> {
    return this._http.post<Page<FVehicle>>('fivepd:vehicle:getAllPageable', collation);
  }
}
