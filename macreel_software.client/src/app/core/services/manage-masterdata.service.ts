import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { ApiResponse, AssignRolePages } from '../models/interface';

@Injectable({
  providedIn: 'root'
})
export class ManageMasterdataService {

  constructor(
    private readonly http: HttpClient
  ) { }

  private readonly baseUrl: string = environment.apiUrl
  private readonly token: string | null = "Get Token";

  AddRole(role: any) {
    return this.http.post(`${this.baseUrl}Master/insertRole`, role, {})
  }

  getRoles(pageNumber: number | null = null, pageSize: number | null = null, searchText: string = '') {
    let params = new HttpParams();

    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchText) params = params.set('searchTerm', searchText); // your API expects searchTerm

    return this.http.get<any>(`${this.baseUrl}Common/getAllRole`, { params, withCredentials:true });
  }

  getRoleById(id: number) {
    return this.http.get<any>(`${this.baseUrl}Master/getRoleById?roleId=${id}`);
  }


  deleteRoleById(id: number) {
    return this.http.delete<any>(`${this.baseUrl}Master/deleteRoleById?roleId=${id}`);
  }

  //DESIGANTION RELATED api
  getDesignation(pageNumber: number | null = null, pageSize: number | null = null, searchText: string = '') {
    let params = new HttpParams();

    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchText) params = params.set('searchTerm', searchText); // your API expects searchTerm

    return this.http.get<any>(`${this.baseUrl}Common/getAllDesignation`, { params });
  }

  addOrUpdateDesignation(role: any) {
    return this.http.post(`${this.baseUrl}Master/insertDesignation`, role, {})
  }

  getDesignationById(id: number) {
    return this.http.get<any>(`${this.baseUrl}Master/getDesignationById?desId=${id}`);
  }

  deleteDesignationById(id: number) {
    return this.http.delete<any>(`${this.baseUrl}Master/deleteDesignationById?desId=${id}`);
  }

  //Department RELATED api
  getDepartment(pageNumber: number | null = null, pageSize: number | null = null, searchText: string = '') {
    let params = new HttpParams();

    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchText) params = params.set('searchTerm', searchText); // your API expects searchTerm

    return this.http.get<any>(`${this.baseUrl}Common/getAllDepartment`, { params });
  }

  addOrUpdateDepartment(role: any) {
    return this.http.post(`${this.baseUrl}Master/insertDepartment`, role, {})
  }

  getDepartmentById(id: number) {
    return this.http.get<any>(`${this.baseUrl}Master/getDepartmentById?depId=${id}`);
  }

  deleteDepartmentById(id: number) {
    return this.http.delete<any>(`${this.baseUrl}Master/deleteDepartmentById?depId=${id}`);
  }

  // ================= TECHNOLOGY APIs =================

  addOrUpdateTechnology(payload: any) {
    return this.http.post<any>(`${this.baseUrl}Master/insertTechnology`, payload);
  }

  // Get All Technology with pagination & search
  getAllTechnology(pageNumber: number | null = null, pageSize: number | null = null, searchText: string = '') {
    let params = new HttpParams();

    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchText) params = params.set('searchTerm', searchText);

    return this.http.get<any>(`${this.baseUrl}Common/GetAllTechnology`, { params });
  }

  // Get Technology By ID
  getTechnologyById(id: number) {
    return this.http.get<any>(`${this.baseUrl}Master/GetAllTechnologyById?id=${id}`);
  }

  // Delete Technology
  deleteTechnologyById(id: number) {
    return this.http.delete<any>(`${this.baseUrl}Master/deleteTechnologyById?id=${id}`);
  }

  //=============Page Services==============
  insertPage(data: any): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}Master/insertPage`,
      data,
      { withCredentials: true }
    );
  }
  getAllPages(pageNumber?: number,pageSize?: number,searchTerm?:string) {

    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (pageNumber !== null && pageNumber !== undefined) {
      params = params.set('pageNumber', pageNumber.toString());
    }

    if (pageSize !== null && pageSize !== undefined) {
      params = params.set('pageSize', pageSize.toString());
    }

    return this.http.get<any>(
      `${this.baseUrl}Master/getAllPages`,
      { params, withCredentials: true }
    );
  }
  getPageById(pageId: number): Observable<any> {
    let params = new HttpParams();

    if (pageId !== null && pageId !== undefined) {
      params = params.set('pageId', pageId.toString());
    }

    return this.http.get<any>(
      `${this.baseUrl}Master/getPageById`,
      { params, withCredentials: true }
    );
  }
  deletePageById(pageId: number): Observable<any> {
    let params = new HttpParams();

    if (pageId !== null && pageId !== undefined) {
      params = params.set('pageId', pageId.toString());
    }

    return this.http.delete<any>(
      `${this.baseUrl}Master/deletePageById`,
      { params, withCredentials: true }
    );
  }

  // Assign Page
  assignRolePages(payload: any): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      `${this.baseUrl}Master/assignRolePages`,
      payload,
      { withCredentials: true }
    );
  }
  getAssignPages(searchTerm?: string|null, pageNumber?: number|null, pageSize?: number|null) {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    if (pageNumber !== null && pageNumber !== undefined) {
      params = params.set('pageNumber', pageNumber.toString());
    }

    if (pageSize !== null && pageSize !== undefined) {
      params = params.set('pageSize', pageSize.toString());
    }
    return this.http.get<any>(`${this.baseUrl}Common/getAllAssignedPages`, {params:params})
  }
  getPagesByRoleId(roleId?: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}Master/get-pages-by-role/${roleId}`, { withCredentials: true });
  }
}
