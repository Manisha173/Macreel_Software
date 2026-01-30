import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult, Task } from '../models/interface';
import { __param } from 'tslib';





@Injectable({
  providedIn: 'root'
})
export class TaskService {

   private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  
getTasks(
  searchTerm: string = '',
  pageNumber: number = 1,
  pageSize: number = 100,
  statusTerm: string = ''
) {
  return this.http.get<PaginatedResult<Task>>(
    `${this.baseUrl}Admin/getAllAssignTask`,
    {
      params: {
        searchTerm,
        pageNumber,
        pageSize,
        statusTerm
      }
    }
  );
}


  deleteTask(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}Admin/deleteTaskAssignById?id=${id}`);
  }

  addTask(formData: FormData): Observable<any> {
    return this.http.post(`${this.baseUrl}Admin/insert-update-Task`, formData);
  }

  //Employee Assigned Tasks
  getAssignedTasks(searchTerm?: string, pageNumber?: number, pageSize?: number): Observable<any> {
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

    return this.http.get<any>(`${this.baseUrl}Employee/AssignedTasks`, { params, withCredentials: true });
  }

  UpdateTaskStatus() {
    return this.http.put<any>(
      `${this.baseUrl}Admin/updateLeaveStatus`,{withCredential:true}
    );
  }
}
