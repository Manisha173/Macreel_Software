import { AfterViewInit, Component, ViewChild, OnInit } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ManageMasterdataService } from '../../../../../core/services/manage-masterdata.service';
import Swal from 'sweetalert2';
import { TableColumn } from '../../../../../core/models/interface';

@Component({
  selector: 'app-add-role',
  standalone: false,
  templateUrl: './add-role.component.html',
  styleUrls: ['./add-role.component.css']
})

export class AddRoleComponent implements OnInit {

  roleName: string = '';
  displayedColumns: string[] = ['srNo', 'name', 'action'];
  dataSource = new MatTableDataSource<PeriodicElement>([]);
  @ViewChild(MatPaginator) paginator!: MatPaginator;


  pageSize: number = 20;

  data: any[] = [];

  pageNumber: number = 1;
  totalRecords: number = 0;
  searchText: string = '';

  editingRoleId: number | null = null;

  constructor(private master: ManageMasterdataService) { }

  ngOnInit(): void {
    this.loadRoles();
  }

     Roles: TableColumn<PeriodicElement>[] = [
      { key: 'name', label: 'Name' },
     
    ];

  loadRoles() {
    this.master.getRoles(this.pageNumber, this.pageSize, this.searchText)
      .subscribe(res => {

        console.log('TOTAL RECORDS 👉', res.totalRecords);
        console.log('PAGE SIZE 👉', this.pageSize);

        const roles = res.data || [];

        this.totalRecords = res.totalRecords; // IMPORTANT
        this.data = roles.map((item: any, index: number) => ({
          srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
          id: item.id,
          name: item.rolename
        }));
      });
  }


  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.pageNumber = event.pageIndex + 1;
    this.loadRoles();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim();
    this.searchText = filterValue;
    this.pageNumber = 1; 
    this.loadRoles();
  }

  onSubmit() {
  if (!this.roleName.trim()) return;

  const payload = { id: this.editingRoleId || 0, rolename: this.roleName };

  this.master.AddRole(payload).subscribe({
    next: () => {
      Swal.fire({
        icon: 'success',
        title: this.editingRoleId
          ? 'Role updated successfully'
          : 'Role added successfully',
        showConfirmButton: false,
        timer: 1500
      });

      this.roleName = '';
      this.editingRoleId = null;
      this.loadRoles();
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


  editRole(role: PeriodicElement) {
    this.master.getRoleById(role.id).subscribe({
      next: (res) => {

        if (res.success && res.data && res.data.length > 0) {

          const roleData = res.data[0];

          this.roleName = roleData.rolename;   
          this.editingRoleId = roleData.id;

        } else {
          Swal.fire({
            icon: 'error',
            title: 'Error!',
            text: 'Role data not found'
          });
        }
      },
      error: () => {
        Swal.fire({
          icon: 'error',
          title: 'Error!',
          text: 'Failed to fetch role data'
        });
      }
    });
  }


  deleteRole(role: PeriodicElement) {
    Swal.fire({
      title: `Are you sure you want to delete ${role.name}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#E6354B',
      cancelButtonColor: '#aaa',
      confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.master.deleteRoleById(role.id).subscribe({
          next: () => {
            Swal.fire({ icon: 'success', title: 'Deleted!', text: `${role.name} has been deleted.`, showConfirmButton: false, timer: 1500 });
            this.loadRoles(); 
          },
          error: () => Swal.fire({ icon: 'error', title: 'Error!', text: 'Failed to delete role' })
        });
      }
    });
  }
}
export interface PeriodicElement {
  srNo: number;
  id: number,
  name: string;
}
