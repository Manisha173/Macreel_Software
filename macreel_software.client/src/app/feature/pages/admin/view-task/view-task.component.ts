import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import Swal from 'sweetalert2';
import { TaskService } from '../../../../core/services/add-task.service';
import { PaginatedList } from '../../../../core/utils/paginated-list';
import { FormBuilder, FormGroup } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { TableColumn, Task } from '../../../../core/models/interface';
import { ActivatedRoute, Router } from '@angular/router';
import { Action } from 'rxjs/internal/scheduler/Action';

@Component({
  selector: 'app-view-task',
  standalone: false,
  templateUrl: './view-task.component.html',
  styleUrls: ['./view-task.component.css']
})
export class ViewTaskComponent implements OnInit {

  searchForm!: FormGroup;
  paginator!: PaginatedList<Task>;
  @ViewChild('filesTemplate', { static: true }) filesTemplate!: TemplateRef<any>;

  showFilesModal = false;
  selectedDocuments: string[] = [];

  taskColumns!: TableColumn<Task>[]; // ⚠️ Initialize later
  pdfUrl: string = 'https://localhost:7253/';
  status: any;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private router: Router,
    private route:ActivatedRoute
  ) { }

  ngOnInit(): void {

     this.route.queryParams.subscribe(params => {
    this.status = params['status']; // could be undefined
  });
    this.searchForm = this.fb.group({ search: [''] });

    // ✅ Initialize columns here AFTER filesTemplate is available
    this.taskColumns = [
      { key: 'title', label: 'Task' },
      { key: 'empName', label: 'Assigned To' },
      { key: 'assignedByName', label: 'Assigned By' },
      { key: 'assignedDate', label: 'Assigned Date', type: 'date', align: 'center' },
      { key: 'completedDate', label: 'Completion Date', type: 'date', align: 'center' },
      { key: 'taskStatus', label: 'Employee Status' },
      {key:'adminTaskStatus',label:'Admin Status'},

      {
        key: 'uploadedDocuments',
        label: 'Files',
        align: 'right',
        type: 'custom',
        template: this.filesTemplate
      }
    ];

    this.paginator = new PaginatedList<Task>(
      20,
      (search, page, size) => this.taskService.getTasks(search, page, size,this.status)
    );

    this.paginator.load();
    setTimeout(() => {
      console.log("data", this.tasks)
    }, (1000));
    this.searchForm.get('search')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(search => {
      this.paginator.reset();
      this.paginator.load(search);
    });
  }
  ngAfterViewInit(): void {
    this.taskColumns = [
      { key: 'title', label: 'Task' },
      { key: 'empName', label: 'Assigned To' },
      { key: 'assignedByName', label: 'Assigned By' },
      { key: 'assignedDate', label: 'Assigned Date', type: 'date', align: 'center' },
      { key: 'completedDate', label: 'Completion Date', type: 'date', align: 'center' },
      { key: 'taskStatus', label: 'Employee Status' },
      {key:'adminTaskStatus',label:'Admin Status'},
  
      {
        key: 'uploadedDocuments',
        label: 'Files',
        align: 'center',
        template: this.filesTemplate
      }
    ];
  }

  
  get tasks(): Task[] 
 {

  if (!this.paginator?.items) return [];

   return  this.paginator.items.map(task => ({
    ...task,
    uploadedDocuments: [task.document1Path, task.document2Path]
      .filter((doc): doc is string => !!doc && doc.trim() !== '')
  }));

    
}
  


openFiles(docs: string[] = []) {
  if (!docs.length) return;

  this.selectedDocuments = docs.map(doc => {
    return `${this.pdfUrl}${doc}`;
  });

  this.showFilesModal = true;
}
  

  closeFiles() {
    this.selectedDocuments = [];
    this.showFilesModal = false;
  }

  onScroll(event: Event): void {
    this.paginator.handleScroll(event, this.searchForm.value.search);
  }

  onEdit(task: Task) {
    this.router.navigate(['/home/admin/add-task'], { state: { task } });
  }

  onDelete(task: Task) {
    Swal.fire({
      title: 'Are you sure?',
      text: `Delete task "${task.title}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, delete',
      cancelButtonText: 'Cancel'
    }).then(result => {
      if (result.isConfirmed) {
        this.taskService.deleteTask(task.id).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'Task has been deleted.', 'success');
            this.paginator.reset();
            this.paginator.load(this.searchForm.value.search);
          },
          error: (err) => {
            Swal.fire('Error', err?.error?.message || 'Failed to delete task', 'error');
          }
        });
      }
    });
  }

}
