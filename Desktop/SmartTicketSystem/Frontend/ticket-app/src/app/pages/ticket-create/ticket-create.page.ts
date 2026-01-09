import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TicketService } from '../../core/services/ticket.service';
import { CategoryService } from '../../core/services/category.service';
import { Category, TICKET_PRIORITIES } from '../../models';

@Component({
  selector: 'app-ticket-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  templateUrl: './ticket-create.page.html',
  styleUrls: ['./ticket-create.page.scss']
})
export class TicketCreatePage implements OnInit {
  form: FormGroup;
  categories: Category[] = [];
  priorities = TICKET_PRIORITIES;
  loading = false;

  constructor(private fb: FormBuilder, private ticket: TicketService, private cat: CategoryService, private router: Router, private snack: MatSnackBar) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      categoryId: ['', Validators.required],
      priority: ['Medium', Validators.required]
    });
  }

  ngOnInit() {
    this.cat.getCategories().subscribe(r => r.success && r.data && (this.categories = r.data.filter(c => c.isActive)));
  }

  submit() {
    if (this.form.valid) {
      this.loading = true;
      this.ticket.createTicket(this.form.value).subscribe({
        next: r => {
          this.loading = false;
          if (r.success) {
            this.snack.open('Ticket created!', 'Close', { duration: 3000 });
            this.router.navigate(['/tickets', r.data?.id]);
          }
        },
        error: e => {
          this.loading = false;
          this.snack.open(e.message, 'Close', { duration: 3000 });
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/tickets']);
  }
}
