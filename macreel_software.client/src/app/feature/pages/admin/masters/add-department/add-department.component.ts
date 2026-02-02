import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import Swal from 'sweetalert2';
import { ManageMasterdataService } from '../../../../../core/services/manage-masterdata.service';
import { TableColumn } from '../../../../../core/models/interface';

@Component({
  selector: 'app-add-department',
  standalone: false,
  templateUrl: './add-department.component.html',
  styleUrls: ['./add-department.component.css']
})
export class AddDepartmentComponent implements OnInit {

  departmentName: string = '';
  editingDepartmentId: number | null = null;

  displayedColumns: string[] = ['srNo', 'name', 'action'];
  dataSource = new MatTableDataSource<DepartmentRow>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  pageSize: number = 20;
  pageNumber: number = 1;
  totalRecords: number = 0;
  searchText: string = '';
  data: any[] = [];
  constructor(private master: ManageMasterdataService) { }

  Roles: TableColumn<DepartmentRow>[] = [
    { key: 'name', label: 'Name' },

  ];
  ngOnInit(): void {
    this.loadDepartments();
  }

  // ================= LOAD DATA =================
  loadDepartments() {
    this.master.getDepartment(
      this.pageNumber,
      this.pageSize,
      this.searchText
    ).subscribe({
      next: (res) => {
        const list = res.data || [];
        this.totalRecords = res.totalRecords || 0;

        this.data = list.map((item: any, index: number) => ({
          srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
          id: item.id,
          name: item.departmentName || item.name
        }));
      },
      error: () => {
        Swal.fire('Error', 'Failed to load departments', 'error');
      }
    });
  }

  // ================= PAGINATION =================
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.pageNumber = event.pageIndex + 1;
    this.loadDepartments();
  }

  // ================= SEARCH =================
  applyFilter(event: Event) {
    this.searchText = (event.target as HTMLInputElement).value.trim();
    this.pageNumber = 1;
    this.loadDepartments();
  }

  // ================= ADD / UPDATE =================
  onSubmit() {
    if (!this.departmentName.trim()) return;

    const payload = {
      id: this.editingDepartmentId || 0,
      departmentName: this.departmentName
    };
    this.master.addOrUpdateDepartment(payload).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: this.editingDepartmentId
            ? 'Department updated'
            : 'Department added',
          timer: 1500,
          showConfirmButton: false
        });

        this.departmentName = '';
        this.editingDepartmentId = null;
        this.loadDepartments();
      },
      error: (err) => {
        console.error('API Error 👉', err);

        const errorMessage =
          err?.error?.message ||
          err?.error?.errorMessage ||
          'Something went wrong';

        Swal.fire({
          icon: 'error',
          title: 'Error!',
          text: errorMessage
        });
      }

    });
  }

  // ================= EDIT =================
  editDepartment(row: DepartmentRow) {
    this.master.getDepartmentById(row.id).subscribe({
      next: (res) => {
     
        if (res.success && res.data?.length) {

          const dept = res.data[0];
       
          this.departmentName = dept.departmentName;
    
          this.editingDepartmentId = dept.id;

        } else {
          Swal.fire('Error', 'Department not found', 'error');
        }
      },
      error: () => {
        Swal.fire('Error', 'Failed to fetch department', 'error');
      }
    });
  }

  // ================= DELETE =================
  deleteDepartment(row: DepartmentRow) {
    Swal.fire({
      title: `Are you sure you want to delete ${row.name}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#C5192F'
    }).then(result => {
      if (result.isConfirmed) {
        this.master.deleteDepartmentById(row.id).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'Deleted',
              timer: 1200,
              showConfirmButton: false
            });
            this.loadDepartments();
          },
          error: () => {
            Swal.fire('Error', 'Failed to delete department', 'error');
          }
        });
      }
    });
  }
}

export interface DepartmentRow {
  srNo: number;
  id: number;
  name: string;
}
