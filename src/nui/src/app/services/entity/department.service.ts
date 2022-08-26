import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, switchMap } from 'rxjs';
import { ActionResponse, Collation, Department, Division, NameTakenResponse, Page, Rank, User } from '@models';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  constructor(
    private _http: HttpClient,
  ) { }

  getDepartment(id: number | string): Observable<Department> {
    if (typeof id === 'string') {
      id = parseInt(id);
    }
    return this._http.post<Department>('fivepd:department:getById', {
      DepartmentId: id,
    });
  }

  getDepartments(departmentsRequest?: Collation): Observable<Page<Department>> {
    return this._http.post<Page<Department>>('fivepd:department:list', departmentsRequest);
  }

  createDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:create', department);
  }

  editDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:edit', department);
  }

  deleteDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:delete', {
      DepartmentId: id,
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
      DepartmentId: id,
    });
  }

  leaveDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:leave', {
      DepartmentId: id,
    });
  }

  createRank(rank: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:createRank', rank);
  }

  editRank(rank: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:editRank', rank);
  }

  deleteRank(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:deleteRank', {
      RankId: id,
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
      DivisionId: id,
    });
  }

  getDepartmentUsers(id: number, collation?: Collation): Observable<Page<User>> {
    return this._http.post<Page<User>>('fivepd:department:getUsers', {
      DepartmentId: id,
      ...collation
    });
  }

  getRanks(departmentId: number, collation?: Collation): Observable<Page<Rank>> {
    return this._http.post<Page<Rank>>('fivepd:department:getRanks', {
      DepartmentId: departmentId,
      ...collation
    });
  }

  getDivisions(id: number, collation?: Collation): Observable<Page<Division>> {
    return this._http.post<Page<Division>>('fivepd:department:getDivisions', {
      DepartmentId: id,
      ...collation
    });
  }
}
