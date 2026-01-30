import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import Swal from 'sweetalert2';
import { ManageMasterdataService } from '../../../../../core/services/manage-masterdata.service';
import { TableColumn } from '../../../../../core/models/interface';
import { PeriodicElement } from '../add-role/add-role.component';

@Component({
  selector: 'app-add-technology',
  standalone: false,
  templateUrl: './add-technology.component.html',
  styleUrl: './add-technology.component.css'
})
export class AddTechnologyComponent implements OnInit {

  softwareType: string = '';
  technologyName: string = '';
  data: any = [];
  displayedColumns: string[] = ['srNo', 'softwareType', 'technologyName', 'action'];
  dataSource = new MatTableDataSource<TechnologyElement>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }

  pageSize: number = 20;
  pageNumber: number = 1;
  totalRecords: number = 0;
  searchText: string = '';

  editingId: number | null = null;

  constructor(private master: ManageMasterdataService) { }

  technolgy: TableColumn<TechnologyElement>[] = [
    { key: 'softwareType', label: 'Software Name' },
    { key: 'technologyName', label: 'Technology Name' },

  ];

  ngOnInit(): void {
    this.loadTechnology();
  }

  loadTechnology() {
    this.master.getAllTechnology(this.pageNumber, this.pageSize, this.searchText)
      .subscribe(res => {

        const data = res.data || [];
        this.totalRecords = res.totalRecords;

        this.data = data.map((item: any, index: number) => ({
          srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
          id: item.id,
          softwareType: item.softwareType,
          technologyName: item.technologyName
        }));
      });
  }

  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.pageNumber = event.pageIndex + 1;
    this.loadTechnology();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim();
    this.searchText = filterValue;
    this.pageNumber = 1;

    // 🔥 IMPORTANT FIX
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }

    this.loadTechnology();
  }


  onSubmit() {
    if (!this.softwareType || !this.technologyName) {
      Swal.fire({ icon: 'warning', title: 'Please fill all fields' });
      return;
    }

    const payload = {
      id: this.editingId || 0,
      softwareType: this.softwareType,
      technologyName: this.technologyName
    };

    this.master.addOrUpdateTechnology(payload).subscribe({
      next: (res) => {
        if (res.success) {
          Swal.fire({
            icon: 'success',
            title: this.editingId ? 'Updated Successfully' : 'Added Successfully',
            showConfirmButton: false,
            timer: 1500
          });
        } else {
          Swal.fire("Warning", res.message, "error")
        }

        this.resetForm();
        this.loadTechnology();
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

  editTechnology(row: TechnologyElement) {
    this.master.getTechnologyById(row.id).subscribe({
      next: (res) => {
        if (res.success && res.data.length > 0) {
          const tech = res.data[0];
          this.softwareType = tech.softwareType;
          this.technologyName = tech.technologyName;
          this.editingId = tech.id;
        }
      }
    });
  }

  deleteTechnology(row: TechnologyElement) {
    Swal.fire({
      title: `Delete ${row.technologyName}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete it!'
    }).then(result => {
      if (result.isConfirmed) {
        this.master.deleteTechnologyById(row.id).subscribe({
          next: () => {
            Swal.fire({ icon: 'success', title: 'Deleted!', timer: 1500, showConfirmButton: false });
            this.loadTechnology();
          }
        });
      }
    });
  }

  resetForm() {
    this.softwareType = '';
    this.technologyName = '';
    this.editingId = null;
  }
}

export interface TechnologyElement {
  srNo: number;
  id: number;
  softwareType: string;
  technologyName: string;
}

