import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
<<<<<<< HEAD
import { LayoutComponent } from '../pages/admin/layout/layout.component';

const routes: Routes = [];
=======
import { ChangePasswordComponent } from './change-password/change-password.component';

const routes: Routes = [
  {path:'',redirectTo:'change-password',pathMatch:'full'},
  {path:'change-password',component:ChangePasswordComponent}
];
>>>>>>> bd0eb45 (Change Paddword UI)

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CommonPagesRoutingModule {
}