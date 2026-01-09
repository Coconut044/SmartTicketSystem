import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-reopen-dialog',
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
    <h2 mat-dialog-title>Reopen Ticket</h2>
    <mat-dialog-content>
      <p class="dialog-description">
        Please explain why this ticket needs to be reopened.
        This will help track recurring issues.
      </p>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Reason for Reopening</mat-label>
          <textarea 
            matInput 
            formControlName="reason" 
            rows="5"
            placeholder="E.g., Issue recurred, Solution did not work, Additional problems found..."
          ></textarea>
          <mat-error *ngIf="form.get('reason')?.hasError('required')">
            Reason is required
          </mat-error>
          <mat-error *ngIf="form.get('reason')?.hasError('minlength')">
            Please provide at least 10 characters
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="warn" 
        [disabled]="form.invalid"
        (click)="onSubmit()"
      >
        Reopen Ticket
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
      min-height: 180px;
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
export class ReopenDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ReopenDialogComponent>
  ) {
    this.form = this.fb.group({
      reason: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value.reason);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}