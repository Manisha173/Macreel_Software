// import { AfterViewInit, Component, ViewChild } from '@angular/core';
// import { MatPaginator } from '@angular/material/paginator';
// import { MatTableDataSource } from '@angular/material/table';
// // import { ManageMasterdataService } from '../../../../core/services/manage-masterdata.service';

// @Component({
//   selector: 'app-add-role',
//   standalone: false,
//   templateUrl: './add-role.component.html',
//   styleUrls: ['./add-role.component.css']
// })
// export class AddRoleComponent implements AfterViewInit {

//   // constructor(
//   //   private master : ManageMasterdataService
//   // ){}

//  displayedColumns: string[] = ['srNo', 'name', 'action'];
//   dataSource = new MatTableDataSource<PeriodicElement>(ELEMENT_DATA);
//   @ViewChild(MatPaginator) paginator!: MatPaginator;

//   ngAfterViewInit() {
//     this.dataSource.paginator = this.paginator;
//   }
//    applyFilter(event: Event) {
//     const filterValue = (event.target as HTMLInputElement).value;
//     this.dataSource.filter = filterValue.trim().toLowerCase();
//   }
// }

// export interface PeriodicElement {
//   srNo: number;
//   name: string;
// }

// const ELEMENT_DATA: PeriodicElement[] = [
// { srNo: 1, name: 'Chief Executive Officer (CEO)' },
//   { srNo: 2, name: 'Chief Technology Officer (CTO)' },
//   { srNo: 3, name: 'Project Manager' },
//   { srNo: 4, name: 'Software Engineer' },
//   { srNo: 5, name: 'Senior Developer' },
//   { srNo: 6, name: 'HR Executive' },
//   { srNo: 7, name: 'Business Analyst' },
//   { srNo: 8, name: 'Marketing Manager' },
//   { srNo: 9, name: 'Accountant' },
//   { srNo: 10, name: 'IT Support Engineer' },
// ];


import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ManageMasterdataService } from '../../../../core/services/manage-masterdata.service';

@Component({
  selector: 'app-add-role',
  standalone: false,
  templateUrl: './add-role.component.html',
  styleUrls: ['./add-role.component.css']
})

export class AddRoleComponent implements AfterViewInit {

  roleName: string = '';

  displayedColumns: string[] = ['srNo', 'name', 'action'];
  dataSource = new MatTableDataSource<PeriodicElement>([]);
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(private master: ManageMasterdataService) {}

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }

  // 🔹 SUBMIT FORM
  onSubmit() {
    if (!this.roleName) return;

    const payload = {
      id: 0,
      rolename: this.roleName  
    };

    this.master.AddRole(payload).subscribe({
      next: () => {
        alert('Role added successfully');

        // table me turant add karne ke liye
        const newRow = {
          srNo: 0,
          name: this.roleName
        };

        this.dataSource.data = [...this.dataSource.data, newRow];
        this.roleName = '';
      },
      error: err => {
        console.error(err);
        alert('Error while adding role');
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }
}

export interface PeriodicElement {
  srNo: number;
  name: string;
}
