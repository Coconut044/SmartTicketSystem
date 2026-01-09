import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CategoryService } from '../../core/services/category.service';
import { UserService } from '../../core/services/user.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { Category, User } from '../../models';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    MatSnackBarModule,
    LoadingComponent,
    MatTooltipModule
  ],
  templateUrl: './admin.page.html',
  styleUrls: ['./admin.page.scss']
})
export class AdminPage implements OnInit {
  categories: Category[] = [];
  users: User[] = [];
  categoryColumns = ['id', 'name', 'description', 'slaHours', 'isActive', 'actions'];
  userColumns = ['id', 'fullName', 'email', 'role', 'isActive', 'createdAt', 'actions'];
  loading = true;
  categoryForm: FormGroup;
  userForm: FormGroup;
  editingCategory: Category | null = null;
  editingUser: User | null = null;

  // ✅ CORRECT - Backend uses "EndUser"
  roles = ['EndUser', 'SupportAgent', 'SupportManager', 'Admin'];

  constructor(
    private cat: CategoryService,
    private userService: UserService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      slaHours: [24, [Validators.required, Validators.min(1)]],
      isActive: [true]
    });

    this.userForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]], // ✅ ADDED
      role: ['EndUser', Validators.required], // ✅ FIXED
      isActive: [true]
    });
  }

  ngOnInit() {
    this.loadCategories();
    this.loadUsers();
  }

  loadCategories() {
    this.loading = true;
    this.cat.getCategories().subscribe({
      next: r => {
        if (r.success && r.data) {
          this.categories = r.data;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadUsers() {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: r => {
        if (r.success && r.data) {
          this.users = r.data;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onCategorySubmit() {
    if (this.categoryForm.invalid) return;

    const data = this.categoryForm.value;
    const obs = this.editingCategory
      ? this.cat.updateCategory(this.editingCategory.id, data)
      : this.cat.createCategory(data);

    obs.subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open(
            this.editingCategory ? 'Category updated' : 'Category created',
            'Close',
            { duration: 3000 }
          );
          this.loadCategories();
          this.resetCategoryForm();
        }
      },
      error: () => this.snackBar.open('Operation failed', 'Close', { duration: 3000 })
    });
  }

  editCategory(cat: Category) {
    this.editingCategory = cat;
    this.categoryForm.patchValue(cat);
  }

  deleteCategory(id: number) {
    if (!confirm('Are you sure you want to delete this category?')) return;

    this.cat.deleteCategory(id).subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open('Category deleted', 'Close', { duration: 3000 });
          this.loadCategories();
        }
      },
      error: () => this.snackBar.open('Delete failed', 'Close', { duration: 3000 })
    });
  }

  resetCategoryForm() {
    this.editingCategory = null;
    this.categoryForm.reset({ slaHours: 24, isActive: true });
  }

  onUserSubmit() {
    if (this.userForm.invalid) return;

    const data = this.userForm.value;
    const obs = this.editingUser
      ? this.userService.updateUser(this.editingUser.id, data)
      : this.userService.createUser(data);

    obs.subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open(
            this.editingUser ? 'User updated' : 'User created',
            'Close',
            { duration: 3000 }
          );
          this.loadUsers();
          this.resetUserForm();
        }
      },
      error: () => this.snackBar.open('Operation failed', 'Close', { duration: 3000 })
    });
  }

  // ✅ When editing, don't require password
  editUser(user: User) {
    this.editingUser = user;
    this.userForm.patchValue(user);

    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
  }

  toggleUserStatus(user: User) {
    this.userService.updateUser(user.id, { ...user, isActive: !user.isActive }).subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open('User status updated', 'Close', { duration: 3000 });
          this.loadUsers();
        }
      },
      error: () => this.snackBar.open('Update failed', 'Close', { duration: 3000 })
    });
  }

  // ✅ Reset form properly
  resetUserForm() {
    this.editingUser = null;
    this.userForm.reset({ role: 'EndUser', isActive: true, password: '' });

    this.userForm.get('password')?.setValidators([
      Validators.required,
      Validators.minLength(6)
    ]);
    this.userForm.get('password')?.updateValueAndValidity();
  }
}
