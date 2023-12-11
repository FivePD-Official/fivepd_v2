import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, FVehicle, FVehicleDTO, Id, Page } from "@models";
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class VehicleService {

  constructor(private readonly _http: HttpClient) {
  }

  get(id: Id): Observable<FVehicleDTO> {
    return this._http.post<FVehicleDTO>('fivepd:vehicle:get', {
      Id: parseId(id),
    });
  }

  getAll(collation?: Collation): Observable<Page<FVehicleDTO>> {
    return this._http.post<Page<FVehicleDTO>>('fivepd:vehicle:getAllPageable', collation);
  }
}
