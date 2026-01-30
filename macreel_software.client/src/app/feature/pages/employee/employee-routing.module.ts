import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AssignedTaskComponent } from './assigned-task/assigned-task.component';
import { ApplyLeaveComponent } from './apply-leave/apply-leave.component';
import { AssignedLeavesComponent } from './assigned-leaves/assigned-leaves.component';
import { AssignProjectComponent } from './assign-project/assign-project.component';
import { TaskListComponent } from './task-list/task-list.component';
import { LayoutComponent } from './layout/layout.component';
import { ProjectProgressComponent } from '../../common-pages/project-progress/project-progress.component';
import { ProjectDetailsComponent } from '../../common-pages/project-details/project-details.component';

const routes: Routes = [
  {
    path: '', component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'assigned-tasks', component: AssignedTaskComponent },
      { path: 'assigned-leaves', component: AssignedLeavesComponent },
      { path: 'apply-leave', component: ApplyLeaveComponent },
      { path: 'assign-project', component: AssignProjectComponent },
      { path: 'task-list', component: TaskListComponent },
      { path: 'project-progress', component: ProjectProgressComponent },
      { path: 'project-details', component: ProjectDetailsComponent }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmployeeRoutingModule { }
