import {
  Component, TemplateRef, ViewChild, OnInit, AfterViewInit
} from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

import { Project, TableColumn } from '../../../core/models/interface';
import { PaginatedList } from '../../../core/utils/paginated-list';
import { ProjectService } from '../../../core/services/project-service.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-project-progress',
  standalone: false,
  templateUrl: './project-progress.component.html',
  styleUrl: './project-progress.component.css'
})
export class ProjectProgressComponent implements OnInit, AfterViewInit {

  @ViewChild('appEmployeeTemplate', { static: true })
  appEmployeeTemplate!: TemplateRef<any>;

  @ViewChild('webEmployeeTemplate', { static: true })
  webEmployeeTemplate!: TemplateRef<any>;
  @ViewChild('iconTemplate', { static: true })
  iconTemplate!: TemplateRef<any>;

  searchForm!: FormGroup;
  paginator!: PaginatedList<Project>;
  projectColumns: TableColumn<Project>[] = [];

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private router: Router,
    private AuthService: AuthService
  ) { }

  // ---------- INIT ----------
  ngOnInit(): void {
    this.searchForm = this.fb.group({
      search: ['']
    });

    this.paginator = new PaginatedList<Project>(
      30,
      (search, page, size) =>
        this.projectService.getProjects(search, page, size, '')
    );

    this.paginator.load();

    this.searchForm.get('search')!.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(search => {
        this.paginator.reset();
        this.paginator.load(search);
      });
  }

  getProjectDetailsRoute(): string {
    const role = this.AuthService.getRole(); // cookie key

    if (role === 'employee') {
      return '/home/employee/project-details';
    }

    if (role === 'admin') {
      return '/home/admin/project-details';
    }

    return '/home';
    
  }


  // ---------- VIEW INIT (Templates SAFE HERE) ----------
  ngAfterViewInit(): void {
    this.projectColumns = [
      {
        key: 'projectTitle',
        label: 'Project',
        clickable: true,
        route: this.getProjectDetailsRoute()
      },
      { key: 'category', label: 'Category' },
      { key: 'startDate', label: 'Start Date', type: 'date', align: 'center' },
      { key: 'endDate', label: 'End Date', type: 'date', align: 'center' },
      {
        key: 'completionDate',
        label: 'Completion Date',
        type: 'date',
        align: 'center'
      },
      {
        key: 'appEmpName',
        label: 'App Employee',
        template: this.appEmployeeTemplate
      },
      {
        key: 'webEmpName',
        label: 'Web Employee',
        template: this.webEmployeeTemplate
      },
      {
        key: 'delayedDays',
        label: 'Status',
        template: this.iconTemplate,
      }
    ];
  }

  // ---------- TABLE SCROLL ----------
  onScroll(event: Event): void {
    this.paginator.handleScroll(event, this.searchForm.value.search);
  }


  // ---------- EDIT ----------
  edit(project: Project): void {
    this.router.navigate(
      ['/home/admin/add-project'],
      { state: { project } }
    );
  }

  // ---------- DELETE ----------
  delete(project: Project): void {
    Swal.fire({
      title: 'Are you sure?',
      text: `Delete project "${project.projectTitle}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete'
    }).then(result => {
      if (result.isConfirmed) {
        this.projectService.delete(project.id).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'Project deleted successfully.', 'success');
            this.paginator.reset();
            this.paginator.load(this.searchForm.value.search);
          },
          error: err => {
            Swal.fire(
              'Error',
              err?.error?.message || 'Failed to delete project',
              'error'
            );
          }
        });
      }
    });
  }
}
