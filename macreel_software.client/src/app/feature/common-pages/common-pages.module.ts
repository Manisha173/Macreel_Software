import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonPagesRoutingModule } from './common-pages-routing.module';
import { AddEmployeeComponent } from './add-employee/add-employee.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { GenericTableComponent } from './generic-table/generic-table.component';
import { CdkAutofill } from "@angular/cdk/text-field";
import { SafeUrlPipe } from "../../core/pipes/capitalize.pipe";
import { ProjectDetailsComponent } from './project-details/project-details.component';
import { EmployeeDetailsComponent } from './employee-details/employee-details.component';
import { ProjectProgressComponent } from './project-progress/project-progress.component';
import { ChangePasswordComponent } from './change-password/change-password.component';



@NgModule({
  declarations: [
    AddEmployeeComponent,
     GenericTableComponent,
     ProjectDetailsComponent,
     EmployeeDetailsComponent,
     ProjectProgressComponent,
     ChangePasswordComponent
  ],
  imports: [
    CommonModule,
    CommonPagesRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    MatIconModule,
    MatFormFieldModule,
    MatChipsModule,
    MatAutocompleteModule,
    MatSelectModule,
    CdkAutofill,
    SafeUrlPipe
],
    exports: [
    GenericTableComponent
  ]
})
export class CommonPagesModule { }
