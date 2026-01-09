import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';
import { USER_ROLES } from '../../models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterLink, 
    MatCardModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatButtonModule, 
    MatIconModule, 
    MatSelectModule, 
    MatProgressSpinnerModule, 
    MatSnackBarModule
  ],
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss']
})
export class RegisterPage {
  form: FormGroup;
  loading = false;
  hide = true;
  roles = USER_ROLES;

  constructor(
    private fb: FormBuilder, 
    private auth: AuthService, 
    private router: Router, 
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['EndUser', Validators.required]
    });
  }

  submit() {
    if (this.form.valid) {
      this.loading = true;
      this.auth.register(this.form.value).subscribe({
        next: r => {
          this.loading = false;
          if (r.success) {
            this.snack.open('Registration successful! Please login.', 'Close', { duration: 3000 });
            // ✅ FIXED: Go to login page after registration
            this.router.navigate(['/login']);
          }
        },
        error: e => {
          this.loading = false;
          this.snack.open(e.message || 'Registration failed', 'Close', { duration: 3000 });
        }
      });
    }
  }
}