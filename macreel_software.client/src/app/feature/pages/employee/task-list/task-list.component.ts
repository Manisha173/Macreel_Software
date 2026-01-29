import { Component, TemplateRef, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TableColumn, Task } from '../../../../core/models/interface';
import { TaskService } from '../../../../core/services/add-task.service';
import { PaginatedList } from '../../../../core/utils/paginated-list';

@Component({
  selector: 'app-task-list',
  standalone: false,
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent {
  searchForm!: FormGroup;
  paginator!: PaginatedList<Task>;
  @ViewChild('filesTemplate', { static: true }) filesTemplate!: TemplateRef<any>;
  showFilesModal = false;
  selectedDocuments: string[] = [];
  taskColumns!: TableColumn<Task>[]; // ⚠️ Initialize later
  pdfUrl: string = 'https://localhost:7253/';

  constructor(
    private readonly fb: FormBuilder,
    private readonly taskService: TaskService,
    private readonly router: Router
  ) { }

  ngOnInit(): void {
    this.searchForm = this.fb.group({ search: [''] });
    // Initialize columns here AFTER filesTemplate is available
    this.taskColumns = [
      { key: 'title', label: 'Task' },
      { key: 'assignedByName', label: 'Assigned By' },
      { key: 'assignedDate', label: 'Assigned Date', type: 'date', align: 'center' },
      { key: 'completedDate', label: 'Completion Date', type: 'date', align: 'center' },
      { key: 'taskStatus', label: 'Employee Status' },
      { key: 'adminTaskStatus', label: 'Admin Status' },
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
      (search, page, size) => this.taskService.getAssignedTasks(search, page, size)
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
      { key: 'assignedByName', label: 'Assigned By' },
      { key: 'assignedDate', label: 'Assigned Date', type: 'date', align: 'center' },
      { key: 'completedDate', label: 'Completion Date', type: 'date', align: 'center' },
      { key: 'taskStatus', label: 'Employee Status' },
      { key: 'adminTaskStatus', label: 'Admin Status' },
      {
        key: 'uploadedDocuments',
        label: 'Files',
        align: 'center',
        template: this.filesTemplate
      }
    ];
  }


  get tasks(): Task[] {
    return this.paginator.items.map(task => ({
      ...task,
      uploadedDocuments: [task.document1Path, task.document2Path].filter((doc): doc is string => !!doc)
    }));
  }

  onScroll(event: Event): void {
    this.paginator.handleScroll(event, this.searchForm.value.search);
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
}
