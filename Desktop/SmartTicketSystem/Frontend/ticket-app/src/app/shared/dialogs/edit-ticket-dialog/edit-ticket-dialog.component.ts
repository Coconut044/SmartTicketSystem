// Create this file: src/app/shared/dialogs/edit-ticket-dialog/edit-ticket-dialog.component.ts

import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CategoryService } from '../../../core/services/category.service';
import { Category, TICKET_PRIORITIES, TicketDetail } from '../../../models';

@Component({
  selector: 'app-edit-ticket-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Edit Ticket</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Title</mat-label>
          <input matInput formControlName="title" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="5"></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Priority</mat-label>
          <mat-select formControlName="priority">
            <mat-option *ngFor="let p of priorities" [value]="p.value">
              {{ p.label }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Category</mat-label>
          <mat-select formControlName="categoryId">
            <mat-option *ngFor="let c of categories" [value]="c.id">
              {{ c.name }}
            </mat-option>
          </mat-select>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="form.invalid">
        Save Changes
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 1rem;
    }
    mat-dialog-content {
      min-width: 500px;
      padding: 1rem;
    }
  `]
})
export class EditTicketDialogComponent implements OnInit {
  form: FormGroup;
  priorities = TICKET_PRIORITIES;
  categories: Category[] = [];

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    public dialogRef: MatDialogRef<EditTicketDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { ticket: TicketDetail }
  ) {
    this.form = this.fb.group({
      title: [data.ticket.title, Validators.required],
      description: [data.ticket.description, Validators.required],
      priority: [data.ticket.priority, Validators.required],
      categoryId: [data.ticket.categoryId, Validators.required]
    });
  }

  ngOnInit() {
    this.categoryService.getCategories().subscribe(r => {
      if (r.success && r.data) {
        this.categories = r.data;
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }

  onSave() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}