import { AfterViewInit, Component, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-add-department',
  standalone: false,
  templateUrl: './add-department.component.html',
  styleUrls: ['./add-department.component.css']
})
export class AddDepartmentComponent implements AfterViewInit{
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
  { srNo: 1, name: 'Finance' },
  { srNo: 2, name: 'Human Resources' },
  { srNo: 3, name: 'IT Department' },
  { srNo: 4, name: 'Human Resources' },
  { srNo: 5, name: 'IT Department' },
  { srNo: 6, name: 'Finance' },
  { srNo: 7, name: 'Human Resources' },
  { srNo: 8, name: 'IT Department' },
  { srNo: 9, name: 'Human Resources' },
  { srNo: 10, name: 'IT Department' },
];
