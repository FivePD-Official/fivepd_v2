import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {
  ActionResponse,
  Collation,
  Department,
  Division, Id,
  NameTakenResponse,
  Page,
  Rank,
  SimpleDepartment, SimpleDepartmentWithArea,
  User
} from '@models';
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  constructor(
    private _http: HttpClient,
  ) {
  }

  getDepartment(id: Id): Observable<Department> {
    return this._http.post<Department>('fivepd:department:get', {
      Id: parseId(id),
    });
  }

  getAllSimple(): Observable<SimpleDepartment[]> {
    return this._http.post<SimpleDepartment[]>('fivepd:department:getAll', {});
  }

  getAllPageable(departmentsRequest?: Collation): Observable<Page<Department>> {
    return this._http.post<Page<Department>>('fivepd:department:getAllPageable', departmentsRequest);
  }

  createDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:create', department);
  }

  editDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:edit', department);
  }

  editCallsignFormat(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:editCallsignFormat', request);
  }

  deleteDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:delete', {
      Id: id,
    });
  }

  checkIfDepartmentNameTaken(name: string, departmentId: number = -1, checkForDepartmentId: boolean = false): Observable<NameTakenResponse> {
    return this._http.post<NameTakenResponse>('fivepd:department:checkIfDepartmentNameTaken', {
      Name: name,
      DepartmentId: departmentId,
      CheckForDepartmentId: checkForDepartmentId,
    });
  }

  joinDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:join', {
      Id: id,
    });
  }

  leaveDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:leave', {
      Id: id,
    });
  }

  kickPlayerFromUI(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:removeUser', {
      Id: id,
    });
  }

  createDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:createDivision', division);
  }

  editDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:renameDivision', division);
  }

  deleteDivision(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:deleteDivision', {
      Id: id,
    });
  }

  getDepartmentUsers(id: number, collation?: Collation): Observable<Page<User>> {
    return this._http.post<Page<User>>('fivepd:department:getUsers', {
      DepartmentId: id,
      ...collation
    });
  }

  getDivision(id: number): Observable<Division> {
    return this._http.post<Division>('fivepd:department:getDivision', {
      Id: id,
    });
  }

  getDivisions(departmentId: number, collation?: Collation): Observable<Page<Division>> {
    return this._http.post<Page<Division>>('fivepd:department:getDivisions', {
      DepartmentId: departmentId,
      ...collation
    });
  }

  getAllSimpleWithArea(): Observable<SimpleDepartmentWithArea[]> {
    return this._http.post<SimpleDepartmentWithArea[]>('fivepd:department:getAllSimpleWithArea', {});
  }

  updateAreas(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:updateAreas', request);
  }
}
