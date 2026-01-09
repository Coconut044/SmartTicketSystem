import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-resolution-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Resolve Ticket</h2>
    <mat-dialog-content>
      <p class="dialog-description">
        Please provide detailed notes explaining how this issue was resolved.
        This information will be visible to the ticket creator.
      </p>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Resolution Notes</mat-label>
          <textarea 
            matInput 
            formControlName="notes" 
            rows="6"
            placeholder="E.g., Replaced faulty hardware component, Updated system configuration, Provided training to user..."
          ></textarea>
          <mat-error *ngIf="form.get('notes')?.hasError('required')">
            Resolution notes are required
          </mat-error>
          <mat-error *ngIf="form.get('notes')?.hasError('minlength')">
            Please provide at least 10 characters
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="form.invalid"
        (click)="onSubmit()"
      >
        Resolve Ticket
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { 
      width: 100%; 
      min-width: 450px; 
      margin-top: 10px;
    }
    mat-dialog-content { 
      padding: 20px 24px;
      min-height: 200px;
    }
    .dialog-description {
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 15px;
      font-size: 14px;
    }
    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class ResolutionDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ResolutionDialogComponent>
  ) {
    this.form = this.fb.group({
      notes: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value.notes);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}