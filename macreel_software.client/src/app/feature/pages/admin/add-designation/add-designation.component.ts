import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-add-designation',
  standalone: false,
  templateUrl: './add-designation.component.html',
  styleUrl: './add-designation.component.css'
})
export class AddDesignationComponent implements AfterViewInit {
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
  { srNo: 1, name: 'Manager' },
  { srNo: 2, name: 'Team Lead' },
  { srNo: 3, name: 'Software Engineer' },
  { srNo: 4, name: 'Senior Developer' },
  { srNo: 5, name: 'HR Executive' },
  { srNo: 6, name: 'Accountant' },
  { srNo: 7, name: 'Marketing Specialist' },
  { srNo: 8, name: 'Project Coordinator' },
  { srNo: 9, name: 'Business Analyst' },
  { srNo: 10, name: 'IT Support Engineer' },
];
