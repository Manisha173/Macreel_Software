import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { AddEmployeeComponent } from '../common-pages/add-employee/add-employee.component';
import { authGuard } from '../../core/guards/guards/auth.guard';
import { roleGuard } from '../../core/guards/guards/role.guard';
import { ProjectDetailsComponent } from '../common-pages/project-details/project-details.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employee-registration', component: AddEmployeeComponent },
  {
    path: 'home',
    canActivate: [authGuard],
    children: [
      { path: '', loadChildren: () => import('../pages/pages.module').then(m => m.PagesModule) },
      {path:'common-pages',loadChildren:()=>import('../common-pages/common-pages.module').then(c=>c.CommonPagesModule)},      
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
