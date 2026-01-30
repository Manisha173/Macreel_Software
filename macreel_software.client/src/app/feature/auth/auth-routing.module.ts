import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { AddEmployeeComponent } from '../common-pages/add-employee/add-employee.component';
import { authGuard } from '../../core/guards/guards/auth.guard';
import { roleGuard } from '../../core/guards/guards/role.guard';
<<<<<<< HEAD
import { ProjectDetailsComponent } from '../common-pages/project-details/project-details.component';
=======
import { CommonModule } from '@angular/common';
>>>>>>> bd0eb45 (Change Paddword UI)

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employee-registration', component: AddEmployeeComponent },
  {
    path: 'home',
    canActivate: [authGuard],
    children: [
      { path: '', loadChildren: () => import('../pages/pages.module').then(m => m.PagesModule) },
<<<<<<< HEAD
      {path:'common-pages',loadChildren:()=>import('../common-pages/common-pages.module').then(c=>c.CommonPagesModule)},      
=======
      { path: 'add-employee', component: AddEmployeeComponent,
        canActivate:[authGuard,roleGuard],
        data:['admin']
       },
       {path:'common',loadChildren:()=>import('../common-pages/common-pages.module').then(n=>n.CommonPagesModule)}
>>>>>>> bd0eb45 (Change Paddword UI)
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
