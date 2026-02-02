

import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { ManageMasterdataService } from '../../../../../core/services/manage-masterdata.service';
import Swal from 'sweetalert2';
import { TableColumn } from '../../../../../core/models/interface';
import { PeriodicElement } from '../add-role/add-role.component';

@Component({
  selector: 'app-add-designation',
  standalone: false,
  templateUrl: './add-designation.component.html',
  styleUrl: './add-designation.component.css'
})
export class AddDesignationComponent implements OnInit {

  designationName = '';
  editingDesignationId: number | null = null;

  displayedColumns: string[] = ['srNo', 'name', 'action'];
  data: any[] = [];
  pageSize = 20;
  pageNumber = 1;
  totalRecords = 0;
  searchText = '';


  constructor(private master: ManageMasterdataService) { }

  Roles: TableColumn<PeriodicElement>[] = [
    { key: 'name', label: 'Name' },

  ];

  ngOnInit(): void {
    this.loadDesignations();
  }

  dataSource: DesignationElement[] = [];
  // ================= LOAD =================
  loadDesignations() {
    this.master
      .getDesignation(this.pageNumber, this.pageSize, this.searchText)
      .subscribe({
        next: (res) => {
          const data = res.data ?? [];

          this.totalRecords = res.totalRecords ?? 0;

          this.data = data.map((item: any, index: number) => ({
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
            id: item.id,
            name: item.designationName
          }));
        },
        error: () => {
          Swal.fire('Error', 'Failed to load designations', 'error');
        }
      });
  }

  // ================= ADD / UPDATE =================
  onSubmit() {
    if (!this.designationName.trim()) return;

    // ✅ Send payload exactly as API expects
    const payload = {
      id: this.editingDesignationId ?? 0,  // 0 for new designation
      designationName: this.designationName
    };

    this.master.addOrUpdateDesignation(payload).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: this.editingDesignationId
            ? 'Designation Updated'
            : 'Designation Added',
          timer: 1500,
          showConfirmButton: false
        });

        this.cancelEdit();
        this.loadDesignations();
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
  editDesignation(row: DesignationElement) {
    this.master.getDesignationById(row.id).subscribe({
      next: (res) => {      
        if (res.success && res.data?.length) {

          const data = res.data[0];

          this.designationName = data.designationName;

          this.editingDesignationId = data.id;

        } else {
          Swal.fire('Error', 'Designation not found', 'error');
        }
      },
      error: () => {
        Swal.fire('Error', 'Failed to fetch designation', 'error');
      }
    });
  }

  cancelEdit() {
    this.designationName = '';
    this.editingDesignationId = null;
  }

  // ================= DELETE =================
  deleteDesignation(row: DesignationElement) {
    Swal.fire({
      title: `Are you sure you want to delete ${row.name}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#C5192F'
    }).then(result => {
      if (result.isConfirmed) {
        this.master.deleteDesignationById(row.id).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'Designation removed', 'success');
            this.loadDesignations();
          },
          error: () => {
            Swal.fire('Error', 'Delete failed', 'error');
          }
        });
      }
    });
  }

  // ================= SEARCH =================
  applyFilter(event: Event) {
    this.searchText = (event.target as HTMLInputElement).value.trim();
    this.pageNumber = 1;
    this.loadDesignations();
  }

  // ================= PAGINATION =================
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.pageNumber = event.pageIndex + 1;
    this.loadDesignations();
  }
}

export interface DesignationElement {
  srNo: number;
  id: number;
  name: string;
}
