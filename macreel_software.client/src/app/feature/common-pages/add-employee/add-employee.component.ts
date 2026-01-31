import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl, }
  from '@angular/forms';
import Swal from 'sweetalert2';
import { finalize } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { ManageEmployeeService } from '../../../core/services/manage-employee.service';
import { ManageMasterdataService } from '../../../core/services/manage-masterdata.service';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';

@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  standalone: false,
  styleUrls: ['./add-employee.component.css'],
})

export class AddEmployeeComponent implements OnInit {

  step = 1;
  isLoading = false;
  roles: any[] = [];
  departments: any[] = [];
  designations: any[] = [];
  states: any[] = [];
  cities: any[] = [];
  reportingManagers: any[] = [];
  technologies: any[] = [];

  isSelfRegistration = false;
  private accessId: string | null = null;

  employeeForm!: FormGroup;
  profilePic?: File;
  aadharImg?: File;
  panImg?: File;

  aadharBackImg?: File;
  panBackImg?: File;

  experienceCertificate?: File;
  tenthCertificate?: File;
  twelthCertificate?: File;
  graduationCertificate?: File;
  mastersCertificate?: File;
  showPassword = false;

  showTechnologySection = false;

  sendLinkForm!: FormGroup;
  isSendingLink = false;
  showSendLinkButton: any;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];
  technologyCtrl = new FormControl('');

  selectedTechnologies: any[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly employeeService: ManageEmployeeService,
    private readonly masterService: ManageMasterdataService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly announcer: LiveAnnouncer,
  ) { }

  employeeId!: number;
  isEditMode = false;

  ngOnInit(): void {
    this.employeeForm = this.fb.group({

      empRoleId: ['', Validators.required],
      empCode: ['', Validators.required],
      empName: ['', Validators.required],
      mobile: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      departmentId: ['', Validators.required],
      designationId: ['', Validators.required],
      emailId: ['', [Validators.required, Validators.email]],
      salary: ['', Validators.required],
      dateOfJoining: ['', Validators.required],
      dob: ['', Validators.required],
      password: ['', Validators.required],
      gender: ['', Validators.required],
      nationality: ['', Validators.required],
      maritalStatus: ['', Validators.required],
      presentAddress: ['', Validators.required],
      stateId: ['', Validators.required],
      cityId: ['', Validators.required],
      reportingManagerId: [''],

      pincode: ['', Validators.required],
      bankName: ['', Validators.required],
      accountNo: ['', Validators.required],
      ifscCode: ['', Validators.required],
      bankBranch: ['', Validators.required],
      emergencyContactPersonName: ['', Validators.required],
      emergencyContactNum: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],

      // IMPORTANT - Multi select array
      skillIds: [[]],

      companyName: [''],
      yearOfExperience: [''],
      technology: [''],
      companyContactNo: ['', Validators.pattern(/^[0-9]{10}$/)],

      profilePic: [null, Validators.required],
      aadharImg: [null, Validators.required],
      panImg: [null, Validators.required],

      aadharBackImg: [null, Validators.required],
      panBackImg: [null, Validators.required],

      addedBy: [1],


    });

    //for send reg link
    this.sendLinkForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    this.loadMasters();
    this.loadStates();
    this.loadReportingManagers();

    this.loadTechnologies();

    this.employeeId = Number(this.route.snapshot.paramMap.get('id'));

    if (this.employeeId) {
      this.isEditMode = true;

      this.disableFileValidationForEdit();
      this.getEmployeeById(this.employeeId);
    } else {
      this.isEditMode = false;
      this.employeeForm.reset();
    }

    this.employeeForm.get('departmentId')?.valueChanges.subscribe((deptId) => {
      this.handleDepartmentChange(deptId);
    });


    this.checkAccessId();
  }

  private handleDepartmentChange(deptId: any): void {
    const selectedDept = this.departments.find(d => d.id == deptId);

    this.showTechnologySection =
      selectedDept?.departmentName === 'Information Technology';

    if (!this.showTechnologySection) {
      this.selectedTechnologies = [];
      this.employeeForm.get('skillIds')?.setValue([]);
    }
  }

  private loadTechnologies(): Promise<void> {
    return new Promise((resolve) => {
      this.masterService.getAllTechnology(1, 100).subscribe((res) => {
        console.log("complte tech data", res)
        this.technologies = res?.data ?? [];
        resolve();
      });
    });
  }

  // get email by accessId
  // private checkAccessId(): void {
  //   const accessId = this.route.snapshot.queryParamMap.get('accessId');
  //   if (accessId) {
  //     this.getEmailByAccessId(accessId);
  //   }

  // }

  private checkAccessId(): void {
    this.accessId = this.route.snapshot.queryParamMap.get('accessId');

    if (this.accessId) {
      this.isSelfRegistration = true;

      this.getEmailByAccessId(this.accessId);
    }
  }


  // get email by accessId
  private getEmailByAccessId(accessId: string): void {
    this.employeeService.getEmailByAccessId(accessId).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.employeeForm.get('emailId')?.patchValue(res.data[0].email);
          this.showSendLinkButton = res.data[0].email;
          console.log("button", this.showSendLinkButton);

        } else {
          Swal.fire('Error', 'Invalid registration link', 'error');
        }
      },
      error: () => {
        Swal.fire('Error', 'Failed to fetch email', 'error');
      },
    });
  }

  private loadMasters(): void {
    this.masterService
      .getRoles(1, 100)
      .subscribe((res) => (this.roles = res?.data ?? []));
    this.masterService
      .getDepartment()
      .subscribe((res) => (this.departments = res?.data ?? []));
    this.masterService
      .getDesignation()
      .subscribe((res) => (this.designations = res?.data ?? []));
  }

  private loadStates(): void {
    this.employeeService.getAllStateList().subscribe((res) => {
      if (res?.status) {
        this.states = res.stateList ?? [];
      }
    });
  }

  onStateChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const stateId = select.value;

    if (!stateId) {
      this.cities = [];
      this.employeeForm.get('cityId')?.setValue('');
      return;
    }

    this.employeeService.getCityByStateId(+stateId).subscribe((res) => {
      if (res?.status) {
        this.cities = res.cityList ?? [];
        this.employeeForm.get('cityId')?.setValue('');
      }
    });
  }

  private loadReportingManagers(): void {
    this.employeeService.getReportingManager().subscribe({
      next: (res) => {
        console.log('FULL API RESPONSE:', res);

        if (res?.success) {
          console.log('ONLY DATA:', res.data);
          this.reportingManagers = res.data ?? [];
        }
      },
      error: (err) => {
        console.error('API ERROR:', err);
      },
    });
  }

  private disableFileValidationForEdit(): void {
    const fileFields = [
      'profilePic',
      'aadharImg',
      'panImg',
      'aadharBackImg',
      'panBackImg',
    ];

    fileFields.forEach(field => {
      const control = this.employeeForm.get(field);
      control?.clearValidators();
      control?.updateValueAndValidity();
    });
  }


  // ================= TECHNOLOGY CHIP LOGIC =================

  onTechnologySelected(event: MatAutocompleteSelectedEvent): void {
    const id = event.option.value;
    const tech = this.technologies.find((t) => t.id === id);

    if (tech && !this.selectedTechnologies.some((t) => t.id === id)) {
      this.selectedTechnologies.push(tech);

      const ids = this.selectedTechnologies.map((t) => t.id);
      this.employeeForm.get('skillIds')?.setValue(ids);
    }

    this.technologyCtrl.setValue('');
    event.option.deselect();
  }

  removeTechnology(tech: any): void {
    this.selectedTechnologies = this.selectedTechnologies.filter(
      (t) => t.id !== tech.id,
    );

    const ids = this.selectedTechnologies.map((t) => t.id);
    this.employeeForm.get('skillIds')?.setValue(ids);

    this.announcer.announce(`Removed ${tech.technologyName}`);
  }

  // ================= EDIT MODE =================

  getEmployeeById(id: number) {
    this.employeeService.getEmployeeById(id).subscribe((res: any) => {
      if (res.success && res.data?.length) {
        const emp = res.data[0];

        this.employeeForm.patchValue({
          empRoleId: Number(emp.empRoleId),
          empCode: emp.empCode,
          empName: emp.empName,
          mobile: emp.mobile,
          emailId: emp.emailId,
          password: emp.password,
          departmentId: Number(emp.departmentId),
          designationId: Number(emp.designationId),
          salary: emp.salary,
          dateOfJoining: emp.dateOfJoining?.substring(0, 10),
          dob: emp.dob?.substring(0, 10),
          gender: emp.gender,
          nationality: emp.nationality,
          maritalStatus: emp.maritalStatus,
          presentAddress: emp.presentAddress,
          stateId: Number(emp.stateId),
          reportingManagerId: Number(emp.reportingManagerId),
          pincode: emp.pincode,
          bankName: emp.bankName,
          accountNo: emp.accountNo,
          ifscCode: emp.ifscCode,
          bankBranch: emp.bankBranch,
          emergencyContactPersonName: emp.emergencyContactPersonName,
          emergencyContactNum: emp.emergencyContactNum,
          companyName: emp.companyName,
          yearOfExperience: emp.yearOfExperience,
          technology: emp.technology,
          companyContactNo: emp.companyContactNo,
        });

        this.handleDepartmentChange(emp.departmentId);


        if (emp.skill && emp.skill.length) {

          this.loadTechnologies().then(() => {

            const skillIds = emp.skill.map((s: any) => s.techId);

            console.log("my tech ID", skillIds);

            this.selectedTechnologies = this.technologies.filter(t =>
              skillIds.includes(t.id)
            );


            this.employeeForm.get('skillIds')?.setValue(skillIds);
          });
        }


        if (emp.stateId) {
          this.employeeService
            .getCityByStateId(emp.stateId)
            .subscribe((res: any) => {
              this.cities = res.cityList ?? [];
              this.employeeForm.get('cityId')?.setValue(emp.cityId);
            });
        }
      }
    });
  }

  nextStep(): void {

    console.log('👉 Next button clicked');
    console.log('Form Value:', this.employeeForm.value);
    console.log('Form Status:', this.employeeForm.status);

    const step1Controls = [
      'empRoleId',
      'empCode',
      'empName',
      'mobile',
      'departmentId',
      'designationId',
      'emailId',
      'salary',
      'dateOfJoining',
      'dob',
      'password',
      'gender',
      'nationality',
      'maritalStatus',
      'presentAddress',
      'stateId',
      'cityId',


      'pincode',
      'bankName',
      'accountNo',
      'ifscCode',
      'bankBranch',
      'emergencyContactPersonName',
      'emergencyContactNum',

      'profilePic',
      'aadharImg',
      'panImg',

      'aadharBackImg',
      'panBackImg',
    ];

    step1Controls.forEach((control) => {
      this.employeeForm.get(control)?.markAsTouched();
    });

    const step1Invalid = step1Controls.some(
      (control) => this.employeeForm.get(control)?.invalid,
    );

    console.log('❌ Invalid Controls:', step1Controls.filter(control => this.employeeForm.get(control)?.invalid));

    if (step1Invalid) {
      Swal.fire(
        'Required Fields Missing',
        'Please fill all mandatory fields',
        'warning',
      );
      return;
    }

    this.step = 2;
  }

  prevStep(): void {
    this.step = 1;
  }

  onFileSelected(event: Event, type: string): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    switch (type) {
      case 'profile':
        this.profilePic = file;
        this.employeeForm.get('profilePic')?.setValue(file);
        break;
      case 'aadhar':
        this.aadharImg = file;
        this.employeeForm.get('aadharImg')?.setValue(file);
        break;

      case 'aadharBack':
        this.aadharBackImg = file;
        this.employeeForm.get('aadharBackImg')?.setValue(file);
        break;

      case 'pan':
        this.panImg = file;
        this.employeeForm.get('panImg')?.setValue(file);
        break;

      case 'panBack':
        this.panBackImg = file;
        this.employeeForm.get('panBackImg')?.setValue(file);
        break;

      case 'experience':
        this.experienceCertificate = file;
        break;
      case 'tenth':
        this.tenthCertificate = file;
        break;
      case 'twelth':
        this.twelthCertificate = file;
        break;
      case 'graduation':
        this.graduationCertificate = file;
        break;
      case 'masters':
        this.mastersCertificate = file;
        break;
    }

  }

  onSubmit(): void {
    if (this.employeeForm.invalid || this.isLoading) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    const rawValue = this.employeeForm.getRawValue();


    const FILE_KEYS = ['profilePic', 'aadharImg', 'panImg'];

    Object.entries(rawValue).forEach(([key, value]) => {
      if (FILE_KEYS.includes(key)) return;

      if (value !== null && value !== undefined && value !== '') {
        if (Array.isArray(value)) {
          formData.append(key, value.join(','));
        } else {
          formData.append(key, value.toString());
        }
      }
    });


    if (this.profilePic) formData.append('ProfilePic', this.profilePic);
    if (this.aadharImg) formData.append('AadharImg', this.aadharImg);
    if (this.panImg) formData.append('PanImg', this.panImg);
    if (this.aadharBackImg)
      formData.append('AadharBackImg', this.aadharBackImg);

    if (this.panBackImg)
      formData.append('PanBackImg', this.panBackImg);

    if (this.experienceCertificate)
      formData.append('ExperienceCertificate', this.experienceCertificate);
    if (this.tenthCertificate)
      formData.append('TenthCertificate', this.tenthCertificate);
    if (this.twelthCertificate)
      formData.append('TwelthCertificate', this.twelthCertificate);
    if (this.graduationCertificate)
      formData.append('GraduationCertificate', this.graduationCertificate);
    if (this.mastersCertificate)
      formData.append('MastersCertificate', this.mastersCertificate);

    this.isLoading = true;

    // 🔥 ADD vs UPDATE decision
    const apiCall = this.isEditMode
      ? this.employeeService.updateEmployee(
        (() => {
          formData.append('Id', this.employeeId.toString());
          return formData;
        })(),
      )
      : this.employeeService.addEmployee(formData);

    apiCall.pipe(finalize(() => (this.isLoading = false))).subscribe({
      // next: (res: any) => {
      //   Swal.fire(
      //     'Success',
      //     res?.message || (this.isEditMode
      //       ? 'Employee updated successfully'
      //       : 'Employee added successfully'),
      //     'success'
      //   ).then(() => {
      //     this.router.navigate(['/home/admin/employee-list']);
      //   });
      // },

      next: (res: any) => {

        if (this.isSelfRegistration) {
          Swal.fire({
            icon: 'success',
            title: 'Registration Successful',
            text: 'Your details have been submitted successfully. Your account will be activated after admin approval.',
            confirmButtonText: 'OK'
          }).then(() => {
            //this.router.navigate(['/login']);
          });

        } else {
          Swal.fire(
            'Success',
            res?.message || (this.isEditMode
              ? 'Employee updated successfully'
              : 'Employee added successfully'),
            'success'
          ).then(() => {
            this.router.navigate(['/home/admin/employee-list']);
          });
        }
      },
      error: (err) => {
        console.log('API ERROR:', err);

        let message = 'Something went wrong. Please try again';

        // ASP.NET Core validation errors
        if (err?.error?.errors) {
          const validationErrors = err.error.errors;

          message = Object.values(validationErrors)
            .flat()
            .join('\n');
        }
        // Normal backend custom message
        else if (err?.error?.message) {
          message = err.error.message;
        }
        // Case 3: Default title
        else if (err?.error?.title) {
          message = err.error.title;
        }

        Swal.fire('Error', message, 'error');
      }
    });
  }

  private resetFiles(): void {
    this.profilePic =
      this.aadharImg =
      this.panImg =
      this.experienceCertificate =
      this.tenthCertificate =
      this.twelthCertificate =
      this.graduationCertificate =
      this.mastersCertificate =
      undefined;
  }
  isModalOpen = false;
  openModal() {
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  //for send registration link
  sendRegistrationLink(): void {
    if (this.sendLinkForm.invalid || this.isSendingLink) {
      this.sendLinkForm.markAllAsTouched();
      return;
    }

    const payload = {
      email: this.sendLinkForm.value.email,
    };

    this.isSendingLink = true;

    this.employeeService
      .sendLinkForReg(payload)
      .pipe(finalize(() => (this.isSendingLink = false)))
      .subscribe({
        next: () => {
          Swal.fire(
            'Success',
            'Registration link sent successfully',
            'success',
          );
          this.sendLinkForm.reset();
          this.closeModal();
        },
        error: () => {
          Swal.fire('Error', 'Failed to send registration link', 'error');
        },
      });
  }
}
