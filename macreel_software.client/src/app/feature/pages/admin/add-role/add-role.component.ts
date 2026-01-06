import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-add-role',
  standalone: false,
  templateUrl: './add-role.component.html',
  styleUrls: ['./add-role.component.css']
})
export class AddRoleComponent implements AfterViewInit {
 displayedColumns: string[] = ['srNo', 'name', 'action'];
  dataSource = new MatTableDataSource<PeriodicElement>(ELEMENT_DATA);
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
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

const ELEMENT_DATA: PeriodicElement[] = [
{ srNo: 1, name: 'Chief Executive Officer (CEO)' },
  { srNo: 2, name: 'Chief Technology Officer (CTO)' },
  { srNo: 3, name: 'Project Manager' },
  { srNo: 4, name: 'Software Engineer' },
  { srNo: 5, name: 'Senior Developer' },
  { srNo: 6, name: 'HR Executive' },
  { srNo: 7, name: 'Business Analyst' },
  { srNo: 8, name: 'Marketing Manager' },
  { srNo: 9, name: 'Accountant' },
  { srNo: 10, name: 'IT Support Engineer' },
];
