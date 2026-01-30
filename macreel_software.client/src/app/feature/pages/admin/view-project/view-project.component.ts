import { AfterViewInit, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ProjectService } from '../../../../core/services/project-service.service';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import Swal from 'sweetalert2';
import { ActivatedRoute, Router } from '@angular/router';
import { Project, TableColumn } from '../../../../core/models/interface';
import { PaginatedList } from '../../../../core/utils/paginated-list';
import { ManageEmployeeService } from '../../../../core/services/manage-employee.service';

@Component({
  selector: 'app-view-project',
  standalone: false,
  templateUrl: './view-project.component.html',
  styleUrls: ['./view-project.component.css']
})
export class ViewProjectComponent implements OnInit, AfterViewInit {

  @ViewChild('iconsTemplate', { static: true }) iconsTemplate!: TemplateRef<any>;
  @ViewChild('filesTemplate', { static: true }) filesTemplate!: TemplateRef<any>;
  @ViewChild('webTemplate', { static: true }) webTemplate!: TemplateRef<any>;

  @ViewChild('appEmployeeTemplate', { static: true }) appEmployeeTemplate!: TemplateRef<any>;
  @ViewChild('webEmployeeTemplate', { static: true }) webEmployeeTemplate!: TemplateRef<any>;

  showEmployeeModal = false;
  selectedEmployees: any[] = [];

  allEmployees: any[] = [];
  employeeTableData: any[] = [];

  showAddEmployeeBox = false;
  employeeSearch = '';
  showEmployeeDropdown = false;
  loadingEmployees = false;

  showAddIcon = false;

  // Static dummy employees
  employees: any[] = [];
  selectedProjectId!: number;
  selectedPmId!: number;
  selectedEmpType!: 'app' | 'web';

  approvedEmployees: any[] = [];
  rejectEmployee: any = null;
  rejectReason = '';
  showRejectModal = false;
  
  searchForm!: FormGroup;
  paginator!: PaginatedList<Project>;
  projectColumns: TableColumn<Project>[] = [];
  employeeColumns!: TableColumn<any>[]; // assign in ngAfterViewInit
  status: any;

  constructor(
    private readonly fb: FormBuilder,
    private readonly projectService: ProjectService,
    private readonly router: Router,
    private readonly employeeService: ManageEmployeeService,
    private readonly route:ActivatedRoute
  ) { }

  ngOnInit(): void {
     this.route.queryParams.subscribe(params => {
    this.status = params['status']; });
    // Project table columns
    this.projectColumns = [
      { key: 'projectTitle', label: 'Project', clickable: true, route: '/home/admin/project-details' },
      { key: 'category', label: 'Category' },
      { key: 'startDate', label: 'Start Date', type: 'date', align: 'center' },
      { key: 'endDate', label: 'End Date', type: 'date', align: 'center' },
      { key: 'completionDate', label: 'Completion Date', type: 'date', align: 'center' },
      {
        key: 'appEmpName',
        label: 'App Employee',
        align: 'right',
        type: 'custom',
        template: this.appEmployeeTemplate
      },
      {
        key: 'webEmpName',
        label: 'Web Employee',
        align: 'right',
        type: 'custom',
        template: this.webEmployeeTemplate
      }
    ];
    this.employeeColumns = [
      { key: 'empName', label: 'Employee Name' },
      { key: 'designation', label: 'Designation' },
      {
        key: 'actions',
        label: 'Actions',
        type: 'custom',
        template: this.iconsTemplate,
        width: '10px'

      }
    ];
    this.searchForm = this.fb.group({ search: [''] });

    this.paginator = new PaginatedList<Project>(
      30,
      (search, page, size) => this.projectService.getProjects(search, page, size,this.status)
    );

    this.paginator.load();

    this.searchForm.get('search')!.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(search => {
        this.paginator.reset();
        this.paginator.load(search);
      });
  }

 
  ngAfterViewInit(): void {

  }

  onScroll(event: Event): void {
    this.paginator.handleScroll(event, this.searchForm.value.search);
  }

  onEmployeeClick(empId: number, id: number, type: 'app' | 'web') {
    if (!empId || empId === 0) return;

    this.selectedPmId = empId;
    this.selectedProjectId = id;
    this.selectedEmpType = type;

    this.showEmployeeModal = true;

    // reset table
    this.employees = [];

    // 🔥 API CALL
    this.getProjectEmployees();
  }

  getProjectEmployees() {
    if (!this.selectedProjectId || !this.selectedPmId) return;

    this.projectService
      .getProjectCoOrdinates(this.selectedProjectId, this.selectedPmId)
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.employees = res.data.map((emp: any) => ({
              id: emp.empId,
              empName: emp.empName,
              designation: emp.designationName,
              isApproved: false
            }));
          } else {
            this.employees = [];
          }
        },
        error: () => {
          Swal.fire('Error', 'Employee list load nahi hui', 'error');
        }
      });
  }






  closeModal() {
    this.showEmployeeModal = false;
    this.selectedEmployees = [];
  }

  toggleAddEmployeeBox() {
    this.showAddEmployeeBox = !this.showAddEmployeeBox;

    if (this.showAddEmployeeBox && this.allEmployees.length === 0) {
      this.fetchEmployees();
    }
  }


  onAddEmployeeClick() {
    this.showEmployeeDropdown = !this.showEmployeeDropdown;

    if (this.showEmployeeDropdown && this.allEmployees.length === 0) {
      this.fetchEmployees();
    }
  }

  fetchEmployees() {
    this.loadingEmployees = true;

    this.employeeService.getAllEmployees(1, 50, '')
      .subscribe(res => {
        if (res.success) {
          this.allEmployees = res.data;
        }
        this.loadingEmployees = false;
      });
  }

  addChip(emp: any) {
    if (!this.selectedEmployees.some(e => e.id === emp.id)) {
      this.selectedEmployees.push({
        id: emp.id,
        empName: emp.empName,
        designation: emp.designationName,
        status: 1
      });
    }

    this.showAddIcon = this.selectedEmployees.length > 0;
  }

  removeChip(emp: any) {
    this.selectedEmployees = this.selectedEmployees.filter(e => e.id !== emp.id);

    this.showAddIcon = this.selectedEmployees.length > 0;
  }

  openEmployeedataModal() {
    this.employeeTableData = [...this.selectedEmployees]; // table stays same
    this.showEmployeeModal = true;
  }



  edit(emp: any) {
    this.router.navigate(['/home/admin/add-project'], { state: { project: emp } });
  }

  Delete(p: Project): void {
    Swal.fire({
      title: 'Are you sure?',
      text: `Delete project "${p.projectTitle}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete'
    }).then(result => {
      if (result.isConfirmed) {
        this.projectService.delete(p.id).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'Project has been deleted.', 'success');
            this.paginator.reset();
            this.paginator.load(this.searchForm.value.search);
          },
          error: (err) => {
            Swal.fire('Error', err?.error?.message || 'Failed to delete project', 'error');
          }
        });
      }
    });
  }

  // approveEmployee(emp: any) {

  //   emp.isApproved = !emp.isApproved; // toggle selection

  //   if (emp.isApproved) {    
  //     this.approvedEmployees.push({
  //       id: emp.id,
  //       empName: emp.empName
  //     });
  //   } else {
  //     this.approvedEmployees =
  //       this.approvedEmployees.filter(e => e.id !== emp.id);
  //   }

  //   this.showAddIcon = this.approvedEmployees.length > 0;
  // }

  approveEmployee(emp: any) {
    emp.isApproved = !emp.isApproved;

    if (emp.isApproved) {
      this.approvedEmployees.push({ id: emp.id, empName: emp.empName });
    } else {
      this.approvedEmployees = this.approvedEmployees.filter(e => e.id !== emp.id);
    }

    console.log('APPROVED LIST 👉', this.approvedEmployees);
  }


  openRejectModal(emp: any) {
    this.rejectEmployee = emp;
    this.rejectReason = '';
    this.showRejectModal = true;
  }
  CancelEmployee() {
  }



  addAndSaveEmployees() {
    this.saveEmployees();
  }
  onSubmitEmployees() {

    if (this.approvedEmployees.length > 0) {
      this.submitApprovedEmployees();
      return;
    }

    if (this.selectedEmployees.length > 0) {
      this.saveEmployees();
      return;
    }


    Swal.fire(
      'Warning',
      'Please select at least one employee',
      'warning'
    );
  }


  saveEmployees() {

    if (!this.selectedEmployees.length) {
      Swal.fire('Warning', 'Please select at least one employee', 'warning');
      return;
    }

    const payload = this.selectedEmployees.map(emp => ({
      ProjectId: this.selectedProjectId,
      PmId: this.selectedPmId,
      EmpId: this.selectedPmId,     // 🔥 OLD employee (important)
      NewEmpId: emp.id,             // 🔥 NEW employee
      Status: 1,                    // 1 = Approve
      Reason: ''
    }));

    console.log('FINAL PAYLOAD 👉', payload);

    this.projectService.updateProjectEmployeeStatus(payload)
      .subscribe({
        next: () => {
          Swal.fire('Success', 'Employee added successfully', 'success');

          this.getProjectEmployees(); // reload list

          // reset UI
          this.selectedEmployees = [];
          this.showEmployeeDropdown = false;
          this.showAddIcon = false;
          this.showEmployeeModal = false;
        },
        error: (err) => {
          console.error(err);
          Swal.fire('Error', 'Employee update failed', 'error');
        }
      });
  }

  submitReject() {
    if (!this.rejectReason.trim()) {
      Swal.fire('Warning', 'Please enter reason', 'warning');
      return;
    }

    const payload = [{
      ProjectId: this.selectedProjectId,
      PmId: this.selectedPmId,
      EmpId: this.rejectEmployee.id,
      NewEmpId: null,
      Status: 2,
      Reason: this.rejectReason
    }];

    console.log('REJECT PAYLOAD 👉', payload);

    this.projectService.updateProjectEmployeeStatus(payload)
      .subscribe({
        next: () => {
          Swal.fire('Rejected', 'Employee rejected successfully', 'success');
          this.showRejectModal = false;
          this.rejectEmployee = null;
          this.rejectReason = '';
          this.getProjectEmployees();
        },
        error: () => {
          Swal.fire('Error', 'Reject failed', 'error');
        }
      });
  }


  submitApprovedEmployees() {

    if (!this.approvedEmployees.length) {
      Swal.fire('Warning', 'Please select at least one employee', 'warning');
      return;
    }

    const payload = this.approvedEmployees.map(emp => ({
      projectId: this.selectedProjectId,
      pmId: this.selectedPmId,
      empId: emp.id,
      newEmpId: null,
      status: 1,
      reason: ''
    }));

    this.projectService.updateProjectEmployeeStatus(payload)
      .subscribe({
        next: () => {
          Swal.fire('Success', 'Employees approved successfully', 'success');

          this.approvedEmployees = [];
          this.getProjectEmployees();
        },
        error: () => {
          Swal.fire('Error', 'Approve failed', 'error');
        }
      });
  }

}