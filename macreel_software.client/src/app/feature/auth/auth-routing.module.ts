import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { AddEmployeeComponent } from '../common-pages/add-employee/add-employee.component';
import { authGuard } from '../../core/guards/guards/auth.guard';
import { roleGuard } from '../../core/guards/guards/role.guard';



const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employee-registration', component: AddEmployeeComponent },
  {
    path: 'home',
    canActivate: [authGuard],
    children: [
      { path: '', loadChildren: () => import('../pages/pages.module').then(m => m.PagesModule) }, 

      { path: 'add-employee', component: AddEmployeeComponent,
        canActivate:[authGuard,roleGuard],
        data:['admin']
       },
       {path:'common',loadChildren:()=>import('../common-pages/common-pages.module').then(n=>n.CommonPagesModule)}

    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
