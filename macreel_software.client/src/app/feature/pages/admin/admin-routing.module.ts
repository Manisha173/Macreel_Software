import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeTaskSheetComponent } from './employee-task-sheet/employee-task-sheet.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { EmployeeListComponent } from './employee-list/employee-list.component';
import { AddEmployeeComponent } from '../../common-pages/add-employee/add-employee.component';
import { AssignLeaveComponent } from './assign-leave/assign-leave.component';
import { UploadAttendanceComponent } from './upload-attendance/upload-attendance.component';
import { ViewAttendanceComponent } from './view-attendance/view-attendance.component';
import { AllEmployeeLeaveListComponent } from './all-employee-leave-list/all-employee-leave-list.component';
import { AddProjectComponent } from './add-project/add-project.component';
import { AddTaskComponent } from './add-task/add-task.component';
import { ViewTaskComponent } from './view-task/view-task.component';
import { ViewProjectComponent } from './view-project/view-project.component';
import { LeaveRequestsComponent } from './leave-requests/leave-requests.component';
import { ProjectDetailsComponent } from '../../common-pages/project-details/project-details.component';
import { EmployeeDetailsComponent } from '../../common-pages/employee-details/employee-details.component';
import { LayoutComponent } from './layout/layout.component';
import { ProjectProgressComponent } from '../../common-pages/project-progress/project-progress.component';

const routes: Routes = [
  {
    path: '', component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'employee-task-sheet', component: EmployeeTaskSheetComponent },
      { path: 'employee-list', component: EmployeeListComponent },
      { path: 'edit-employee/:id', component: AddEmployeeComponent },
      { path: 'assign-leave', component: AssignLeaveComponent },
      { path: 'upload-attendance', component: UploadAttendanceComponent },
      { path: 'view-attendance', component: ViewAttendanceComponent },
      { path: 'assigned-employees-leaves', component: AllEmployeeLeaveListComponent },
      { path: 'add-task', component: AddTaskComponent },
      { path: 'view-task', component: ViewTaskComponent },
      { path: 'view-project', component: ViewProjectComponent },
      { path: 'add-project', component: AddProjectComponent },
      { path: 'leave-requests', component: LeaveRequestsComponent },
      { path: 'master', loadChildren: () => import('./masters/masters.module').then(n => n.MastersModule) },
      { path: 'project-details', component: ProjectDetailsComponent },
      { path: 'employee-details', component: EmployeeDetailsComponent },
      { path: 'employee-details', component: EmployeeDetailsComponent },
      { path: 'add-employee', component: AddEmployeeComponent },
      { path: 'project-progress', component: ProjectProgressComponent },
      { path: 'edit-employee/:id', component: AddEmployeeComponent },
      { path: 'project-details', component: ProjectDetailsComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
