import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult, Project } from '../models/interface';

@Injectable({ providedIn: 'root' })
export class ProjectService {

  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) { }

  // Get projects with table-only loader (disable global loader)
  getProjects(search: string, page: number, size: number,status:string =''): Observable<PaginatedResult<Project>> {
    let params = new HttpParams()
      .set('searchTerm', search)
      .set('pageNumber', page)
      .set('status', status)
      .set('pageSize', size);
      

    return this.http.get<PaginatedResult<Project>>(`${this.baseUrl}Common/getAllProject`, { params });
  }

  // Delete project with table-only loader
  delete(id: number): Observable<any> {
    const headers = new HttpHeaders({
      'X-Skip-Global-Loader': 'true'  // Skip global loader here too
    });

    return this.http.delete(`${this.baseUrl}Admin/deleteProjectById?id=${id}`, { headers });
  }

  // Get assigned projects for logged-in employee
  getAssignedProjectsByEmp(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.baseUrl}Employee/AssignedProjectByEmpId`);
  }

  assignProjectToEmp(payload: {
    projectId: number;
    empIds: string;
  }) {
    return this.http.post<any>(
      `${this.baseUrl}Common/AssignProjectToEmp`,
      payload
    );
  }

  getAssignedProjectEmpList(projectId: number) {
    return this.http.get<any>(
      `${this.baseUrl}Common/AssignedProjectEmpList`,
      { params: { projectId } }
    );
  }

  getProjectCoOrdinates(projectId: number, pmId: number) {
    return this.http.get<any>(
      `${this.baseUrl}Admin/ProjectCoOrdinateList`,
      {
        params: {
          projectId: projectId.toString(),
          pmId: pmId.toString()
        }
      }
    );
  }

 updateProjectEmployeeStatus(payload: any) {
  return this.http.post(
    `${this.baseUrl}Common/update-project-emp-status`,
    payload
  );
}


}
