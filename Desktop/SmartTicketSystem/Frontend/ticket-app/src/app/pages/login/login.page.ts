import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service'; // 🔥 ADD THIS

@Component({
  selector: 'app-login',
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
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss']
})
export class LoginPage {

  hide = true;
  loading = false;

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private notificationSvc: NotificationService, // 🔥 ADD THIS
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  submit() {
    if (this.loginForm.invalid || this.loading) return;

    this.loading = true;

    const data = {
      email: this.loginForm.value.email!,
      password: this.loginForm.value.password!
    };

    this.auth.login(data)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => {
          if (!res.success || !res.data) {
            this.snackBar.open('Login failed', 'Close', { duration: 3000 });
            return;
          }

          // ✅ Save auth data
          this.auth.saveAuthData(res);

          // 🔥 TRIGGER NOTIFICATIONS LOAD
          this.notificationSvc.getUnreadCount().subscribe();

          const role = res.data.user.role;

          // ✅ ROLE-BASED REDIRECT
          switch (role) {
            case 'Admin':
              this.router.navigate(['/admin']);
              break;

            case 'SupportManager':
              this.router.navigate(['/reports']);
              break;

            case 'SupportAgent':
              this.router.navigate(['/queue']);
              break;

            default:
              this.router.navigate(['/my-tickets']);
          }

          this.snackBar.open('Welcome back!', 'Close', { duration: 3000 });
        },
        error: err => {
          console.error('Login error:', err);
          this.snackBar.open(
            err.error?.message || 'Login failed',
            'Close',
            { duration: 3000 }
          );
        }
      });
  }
}
