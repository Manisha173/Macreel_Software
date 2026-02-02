import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ManageMasterdataService } from '../../../../core/services/manage-masterdata.service';
import Swal from 'sweetalert2';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AddProjectService } from '../../../../core/services/add-project.service';
import { Project } from '../../../../core/models/interface';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-project',
  standalone: false,
  templateUrl: './add-project.component.html',
  styleUrls: ['./add-project.component.css']
})

export class AddProjectComponent implements OnInit {
  isEditMode: boolean = false;
  editProjectId: number = 0;

  @ViewChild('sopFile') sopFile!: ElementRef<HTMLInputElement>;
  @ViewChild('technicalFile') technicalFile!: ElementRef<HTMLInputElement>;

  projectForm!: FormGroup;

  allTechnologies: any[] = [];
  mobileSkills: any[] = [];
  webSkills: any[] = [];
  filteredMobileEmployees: any[] = [];
  filteredWebEmployees: any[] = [];

  selectedEmployeeProjects: any[] = [];
  selectedEmployeeDetails: any = null;

  existingSopPath: string | null = null;
  existingTechnicalPath: string | null = null;

  newSopFile: File | null = null;
  newTechnicalFile: File | null = null;

  showMobilePlatformError = false;
  showMobileSkillError = false;
  showMobileEmpError = false;
  showSoftwareTypeError = false;

  showWebSkillError = false;
  showWebEmpError = false;


  // ========== DATE MIN VALUES ==========
  minAssignDate: string = '';
  minEndDate: string = '';
  minCompletionDate: string = '';

  constructor(
    private fb: FormBuilder,
    private masterService: ManageMasterdataService,
    private addProjectService: AddProjectService,
    private router: Router
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadTechnologies();

    const nav = this.router.getCurrentNavigation();
    const project = history.state?.project;
    console.log('Received project data:', project);
    if (project) {
      this.bindEditData(project);
    }

    // ================== DATE RESTRICTIONS ==================
    this.projectForm.get('startDate')?.valueChanges.subscribe(startDate => {
      if (startDate) {
        const minDate = this.formatDate(new Date(startDate));
        this.minAssignDate = minDate;
        this.minEndDate = minDate;
        this.minCompletionDate = minDate;

        // Reset dates if before startDate
        this.resetIfBefore('assignDate', new Date(startDate));
        this.resetIfBefore('endDate', new Date(startDate));
        this.resetIfBefore('completionDate', new Date(startDate));
      } else {
        this.minAssignDate = '';
        this.minEndDate = '';
        this.minCompletionDate = '';
      }
    });

    this.projectForm.get('mobileSkill')?.valueChanges.subscribe(val => {
      this.onMobileSkillChange(val);
    });

    this.projectForm.get('mobileEmpId')?.valueChanges.subscribe(() => {
      this.onSelectMobileEmployee();
    });

    this.projectForm.get('webSkill')?.valueChanges.subscribe(val => {
      this.onWebSkillChange(val);
    });

    this.projectForm.get('webEmpId')?.valueChanges.subscribe(() => {
      this.onSelectWebEmployee();
    });
  }

  initForm() {
    this.projectForm = this.fb.group({
      category: ['', Validators.required],
      projectTitle: ['', Validators.required],
      description: [''],
      startDate: ['', Validators.required],
      assignDate: ['', Validators.required],
      endDate: [''],
      completionDate: [''],
      // software type
      isMobileSoftware: [false],
      isWebSoftware: [false],
      isAndroid: [false],
      isIOS: [false],
      // mobile
      mobileSkill: [''],
      mobileEmpId: [''],
      // web
      webSkill: [''],
      webEmpId: [''],
      // digital marketing
      SEO: [''],
      SMO: [''],
      GMB: [''],
      paidAds: ['']
    });
  }

  get category(): 'Software' | 'Website' | 'Digital Marketing' | '' {
    return this.projectForm.get('category')?.value || '';
  }

  get isMobileSoftware(): boolean {
    return this.projectForm.get('isMobileSoftware')?.value;
  }

  get isWebSoftware(): boolean {
    return this.projectForm.get('isWebSoftware')?.value;
  }
  get isAndroid(): boolean {
    return this.projectForm.get('isAndroid')?.value;
  }

  get isIOS(): boolean {
    return this.projectForm.get('isIOS')?.value;
  }

  // ================= LOAD TECHNOLOGIES =================
  loadTechnologies() {
    this.masterService.getAllTechnology(1, 100).subscribe({
      next: (res: any) => {
        this.allTechnologies = res?.data || [];
        this.mobileSkills = this.allTechnologies.filter(t => t.softwareType?.toLowerCase() === 'app');
        this.webSkills = this.allTechnologies.filter(t => t.softwareType?.toLowerCase() === 'web');
      },
      error: err => console.error(err)
    });
  }

  onCategoryChange() {
    this.projectForm.patchValue({
      isMobileSoftware: false,
      isWebSoftware: false,
      isAndroid: false,
      isIOS: false,
      mobileSkill: '',
      webSkill: '',
      mobileEmpId: '',
      webEmpId: '',
      SEO: '',
      SMO: '',
      GMB: '',
      paidAds: ''
    });
    this.filteredMobileEmployees = [];
    this.filteredWebEmployees = [];
  }

  onMobileSkillChange(skillId: any) {
    const id = skillId ? Number(skillId) : null;
    if (!id) {
      this.filteredMobileEmployees = [];
      return;
    }

    this.addProjectService.getEmpListForAppByTechId(id).subscribe({
      next: res => {
        this.filteredMobileEmployees = res?.data || [];

        if (this.filteredMobileEmployees.length === 0) {
          Swal.fire({
            icon: 'info',
            title: 'No Employee Found',
            text: 'No employee is available for this selected skill.',
          });
        }
      },
      error: err => {
        this.filteredMobileEmployees = [];

        Swal.fire({
          icon: 'error',
          title: 'Error',
          text: 'Unable to fetch employee list. Please try again.',
        });
      }
    });
  }

  onSelectMobileEmployee() {
    const empId = this.projectForm.get('mobileEmpId')?.value;
    if (!empId) return;

    const id = Number(empId);
    this.addProjectService.getProjectDetailsByEmpId(id).subscribe({
      next: res => this.selectedEmployeeProjects = res?.data || [],
      error: () => this.selectedEmployeeProjects = []
    });
  }

  onWebSkillChange(skillId: any) {
    const id = skillId ? Number(skillId) : null;

    if (!id) {
      this.filteredWebEmployees = [];
      return;
    }
    this.addProjectService.getEmpListForWebByTechId(id).subscribe({
      next: res => {
        this.filteredWebEmployees = res?.data || [];

        if (this.filteredWebEmployees.length === 0) {
          Swal.fire({
            icon: 'info',
            title: 'No Employee Found',
            text: 'No employee is available for this selected skill.',
          });
        }
      },
      error: err => {
        this.filteredWebEmployees = [];

        Swal.fire({
          icon: 'error',
          title: 'Error',
          text: 'Unable to fetch employee list. Please try again.',
        });
      }
    });
  }

  onSelectWebEmployee(): void {
    const empId = this.projectForm.get('webEmpId')?.value;
    console.log('Selected Web Employee ID:', empId);
    if (!empId) return;

    const id = Number(empId); // convert to number

    this.addProjectService.getProjectDetailsByEmpId(id).subscribe({
      next: res => this.selectedEmployeeProjects = res?.data || [],
      error: () => this.selectedEmployeeProjects = []
    });
  }

  onSopFileChange(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const allowedTypes = ['application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];

    if (!allowedTypes.includes(file.type)) {
      Swal.fire({
        icon: 'error',
        title: 'Invalid File',
        text: 'Upload only .pdf, .doc, .docx file!',
      });

      this.sopFile.nativeElement.value = ''; // reset input
      this.newSopFile = null;
      return;
    }

    this.newSopFile = file;
  }

  onTechnicalFileChange(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const allowedTypes = ['application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];

    if (!allowedTypes.includes(file.type)) {
      Swal.fire({
        icon: 'error',
        title: 'Invalid File',
        text: 'Upload only .pdf, .doc, .docx file!',
      });

      this.technicalFile.nativeElement.value = '';
      this.newTechnicalFile = null;
      return;
    }
    this.newTechnicalFile = file;
  }

  bindEditData(emp: any) {
    this.isEditMode = true;
    this.editProjectId = emp.id;

    this.existingSopPath = emp.sopDocumentPath;
    this.existingTechnicalPath = emp.technicalDocumentPath;
    // BASIC DATA
    this.projectForm.patchValue({
      id: emp.id,
      category: emp.category,
      projectTitle: emp.projectTitle,
      description: emp.description,

      startDate: emp.startDate?.split('T')[0],
      assignDate: emp.assignDate?.split('T')[0],
      endDate: emp.endDate?.split('T')[0],
      completionDate: emp.completionDate?.split('T')[0],

      // isMobileSoftware: emp.app === 'App',
      // isWebSoftware: emp.web === 'Web' || emp.category === 'Website',
      // isAndroid: emp.androidApp === 'Android',
      // isIOS: emp.iosApp === 'IOS',

      isMobileSoftware: !!emp.app,
      isWebSoftware: !!emp.web,
      isAndroid: !!emp.androidApp,
      isIOS: !!emp.iosApp,

      SEO: emp.seo,
      SMO: emp.smo,
      GMB: emp.gmb,
      paidAds: emp.paidAds
    })
    this.existingSopPath = emp.sopDocumentPath;
    this.existingTechnicalPath = emp.technicalDocumentPath;

    // MOBILE
    if (emp.appTechnology) {
      this.projectForm.patchValue({ mobileSkill: emp.appTechnology });

      this.addProjectService
        .getEmpListForAppByTechId(emp.appTechnology)
        .subscribe(res => {
          this.filteredMobileEmployees = res?.data || [];
          this.projectForm.patchValue({ mobileEmpId: emp.appEmpId });
        });
    }

    // WEB
    if (emp.webTechnology) {
      this.projectForm.patchValue({ webSkill: emp.webTechnology });

      this.addProjectService
        .getEmpListForWebByTechId(emp.webTechnology)
        .subscribe(res => {
          this.filteredWebEmployees = res?.data || [];
          this.projectForm.patchValue({ webEmpId: emp.webEmpId });
        });
    }
  }

  clearFieldError(type: 'mobilePlatform' | 'mobileSkill' | 'mobileEmp' | 'webSkill' | 'webEmp') {
    switch (type) {
      case 'mobilePlatform':
        this.showMobilePlatformError = false;
        break;

      case 'mobileSkill':
        this.showMobileSkillError = false;
        break;

      case 'mobileEmp':
        this.showMobileEmpError = false;
        break;

      case 'webSkill':
        this.showWebSkillError = false;
        break;

      case 'webEmp':
        this.showWebEmpError = false;
        break;
    }
  }
  clearSoftwareTypeError() {
    this.showSoftwareTypeError = false;
  }


  // ================= FORM SUBMISSION =================
  submitProject() {

    if (this.projectForm.invalid) {

      // ✅ EDIT MODE FIX — SOP validation bypass
      if (this.isEditMode && this.existingSopPath) {
        console.log('Edit mode: allowing submit with existing SOP');
      }
      else {
        Swal.fire({
          icon: 'warning',
          title: 'Required Fields Missing',
          text: 'Please fill required fields',
        });
        return;
      }
    }
    let hasError = false;
    this.resetErrors();

    const formData = new FormData();
    const f = this.projectForm.value;

    // if (f.isMobileSoftware) {
    //   if (!f.isAndroid && !f.isIOS) {
    //     Swal.fire({
    //       icon: 'warning',
    //       title: 'Selection Required',
    //       text: 'Please select at least one: Android or iOS',
    //     });
    //     return;
    //   }
    // }

    // ========== MOBILE VALIDATION ==========
    if (f.isMobileSoftware) {

      if (!f.isAndroid && !f.isIOS) {
        this.showMobilePlatformError = true;
        hasError = true;
      }

      if ((f.isAndroid || f.isIOS) && !f.mobileSkill) {
        this.showMobileSkillError = true;
        hasError = true;
      }

      if (f.mobileSkill && !f.mobileEmpId) {
        this.showMobileEmpError = true;
        hasError = true;
      }

    }
    // ========== SOFTWARE TYPE VALIDATION ==========
    if (f.category === 'Software' || f.category === 'Website') {
      if (!f.isMobileSoftware && !f.isWebSoftware) {
        this.showSoftwareTypeError = true;
        hasError = true;
      }
    }

    // ========== WEB VALIDATION ==========
    if (f.isWebSoftware) {

      if (!f.webSkill) {
        this.showWebSkillError = true;
        hasError = true;
      }

      if (f.webSkill && !f.webEmpId) {
        this.showWebEmpError = true;
        hasError = true;
      }
    }

    if (hasError) return;

    // ================= BASIC FIELDS =================
    formData.append('id', this.editProjectId.toString());
    formData.append('category', f.category);
    formData.append('projectTitle', f.projectTitle);
    formData.append('description', f.description || '');
    formData.append('startDate', f.startDate);
    formData.append('assignDate', f.assignDate);
    formData.append('endDate', f.endDate || '');
    formData.append('completionDate', f.completionDate || '');

    // ================= SOFTWARE TYPE =================
    if (f.isMobileSoftware) {
      //   formData.append('app', 'App');

      // if (f.isAndroid) {
      //   formData.append('androidApp', 'Android');
      // }

      // if (f.isIOS) {
      //   formData.append('iosApp', 'IOS');
      // }
      formData.append('app', f.isMobileSoftware.toString());
      formData.append('androidApp', f.isAndroid.toString());
      formData.append('iosApp', f.isIOS.toString());

      if (f.mobileSkill) {
        formData.append('appTechnology', f.mobileSkill);
      }

      if (f.mobileEmpId) {
        formData.append('appEmpId', f.mobileEmpId);
      }
    }

    if (f.isWebSoftware) {
      //   formData.append('web', 'Web');

      formData.append('web', f.isWebSoftware.toString());

      if (f.webSkill) {
        formData.append('webTechnology', f.webSkill);
      }

      if (f.webEmpId) {
        formData.append('webEmpId', f.webEmpId);
      }
    }

    // ================= DIGITAL MARKETING =================
    if (f.category === 'Digital Marketing') {
      formData.append('SEO', f.SEO || '');
      formData.append('SMO', f.SMO || '');
      formData.append('GMB', f.GMB || '');
      formData.append('paidAds', f.paidAds || '');
    }

    // ================= FILES (CORRECT WAY) =================

    // ---- SOP DOCUMENT ----
    if (this.newSopFile) {
      formData.append("sopDocument", this.newSopFile);
    }
    // ❌ REMOVE THIS (WRONG APPROACH)
    // else if (this.isEditMode && this.existingSopPath) {
    //   formData.append("sopDocument", this.existingSopPath);
    // }

    // ---- TECHNICAL DOCUMENT ----
    if (this.newTechnicalFile) {
      formData.append("technicalDocument", this.newTechnicalFile);
    }
    // ❌ REMOVE THIS TOO
    // else if (this.isEditMode && this.existingTechnicalPath) {
    //   formData.append("technicalDocument", this.existingTechnicalPath);
    // }

    // ================= API CALL =================
    this.addProjectService.addProject(formData).subscribe({
      next: res => {
        if (res.success) {
          Swal.fire({
            icon: 'success',
            title: 'Project Added',
            text: res.message,
            confirmButtonColor: '#3085d6',
          });

          this.projectForm.reset();
          this.sopFile.nativeElement.value = '';
          this.technicalFile.nativeElement.value = '';
          this.selectedEmployeeProjects = [];
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: res.message || 'Error adding project',
          });
        }
      },

      // ✅ FIXED ERROR HANDLING — BACKEND MESSAGE SHOW HOGA
      error: (err) => {

        let message = 'Something went wrong!';

        if (err?.error) {
          if (typeof err.error === 'string') {
            message = err.error;
          }
          else if (err.error.message) {
            message = err.error.message;
          }
        }

        Swal.fire({
          icon: 'error',
          title: 'Validation Error',
          text: message,
        });
      }
    });
  }

  formatDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = ('0' + (date.getMonth() + 1)).slice(-2);
    const dd = ('0' + date.getDate()).slice(-2);
    return `${yyyy}-${mm}-${dd}`;
  }
  resetIfBefore(field: string, minDate: Date) {
    const current = this.projectForm.get(field)?.value;
    if (current && new Date(current) < minDate) {
      this.projectForm.patchValue({ [field]: '' });
    }
  }
  resetErrors() {
    this.showMobilePlatformError = false;
    this.showMobileSkillError = false;
    this.showMobileEmpError = false;
    this.showWebSkillError = false;
    this.showWebEmpError = false;
    this.showSoftwareTypeError = false;
  }
}

