import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { environment, pdfUrl } from '../../../../environments/environment';

export interface EmployeeDetails {
  id: string;
  empRoleId: string;
  roleName: string;
  empCode: string;
  empName: string;
  mobile: string;
  departmentId: string;
  departmentName: string;
  designationId: string;
  designationName: string;

  // reportingManagerId: string;
  reportingManagerName: string;

  emailId: string;
  dateOfJoining: string;
  yearOfExperience: number;
  salary: string;
  bankName: string;
  accountNo: string;
  ifscCode: string;
  bankBranch: string;
  dob: string;
  gender: string;
  nationality: string;
  maritalStatus: string;
  presentAddress: string;
  stateId: string;
  stateName: string;
  cityId: string;
  cityName: string;
  pincode: string;
  emergencyContactPersonName: string;
  emergencyContactNum: string;
  companyName: string;
  profilePicPath: string;
  aadharImgPath: string;
  aadharBackImgPath: string;
  panImgPath: string;
  panBackImgPath: string;
  experienceCertificatePath: string;
  tenthCertificatePath: string;
  twelthCertificatePath: string;
  graduationCertificatePathan: string;
  mastersCertificatePath: string;
  skill: any[];
  companyContactNo: number;
  password:string;
}

@Component({
  selector: 'app-employee-details',
  standalone: false,
  templateUrl: './employee-details.component.html',
  styleUrls: ['./employee-details.component.css']
})
export class EmployeeDetailsComponent implements OnInit {

  apiUrl = environment.apiUrl;
  pdfBaseUrl = pdfUrl.pdfUrl;
  employee!: EmployeeDetails;

  constructor(private readonly route: ActivatedRoute) { }

  ngOnInit(): void {
    this.employee = history.state.employee;
    console.log(this.employee)
  }
}
