import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { TicketService } from '../../core/services/ticket.service';
import { CommentService } from '../../core/services/comment.service';
import { LifecycleService } from '../../core/services/lifecycle.service';
import { AuthService } from '../../core/services/auth.service';

import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../shared/components/priority-badge/priority-badge.component';
import { EditTicketDialogComponent } from '../../shared/dialogs/edit-ticket-dialog/edit-ticket-dialog.component';

import { TicketDetail } from '../../models';

@Component({
  selector: 'app-ticket-view',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatIconModule,
    MatMenuModule,
    MatDialogModule,
    MatSnackBarModule,
    LoadingComponent,
    StatusBadgeComponent,
    PriorityBadgeComponent
  ],
  templateUrl: './ticket-view.page.html',
  styleUrls: ['./ticket-view.page.scss']
})
export class TicketViewPage implements OnInit {

  ticket: TicketDetail | null = null;
  commentForm: FormGroup;
  loading = true;
  id!: number;
  currentUserId = 0;
  currentUserRole = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ticketSvc: TicketService,
    private commentSvc: CommentService,
    private lifecycle: LifecycleService,
    public auth: AuthService,
    private fb: FormBuilder,
    private dialog: MatDialog,
    private snack: MatSnackBar
  ) {
    this.commentForm = this.fb.group({
      comment: ['', Validators.required],
      isInternal: [false]
    });

    const user = this.auth.getCurrentUser();
    if (user) {
      this.currentUserId = user.id;
      this.currentUserRole = user.role;
    }
  }

  ngOnInit() {
    this.id = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    this.loading = true;
    this.ticketSvc.getTicketById(this.id).subscribe({
      next: r => {
        if (r.success && r.data) this.ticket = r.data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  // ==================== CREATE ====================
  addComment() {
    if (this.commentForm.valid) {
      this.commentSvc.addComment(this.id, this.commentForm.value).subscribe({
        next: () => {
          this.snack.open('Comment added', 'Close', { duration: 3000 });
          this.commentForm.reset({ isInternal: false });
          this.load();
        },
        error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
      });
    }
  }

  // ==================== UPDATE ====================
  editTicket() {
    const dialogRef = this.dialog.open(EditTicketDialogComponent, {
      width: '600px',
      data: { ticket: this.ticket }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.ticketSvc.updateTicket(this.id, result).subscribe({
          next: r => {
            if (r.success) {
              this.snack.open('Ticket updated successfully', 'Close', { duration: 3000 });
              this.load();
            }
          },
          error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
        });
      }
    });
  }

  startWork() {
    this.lifecycle.startWork(this.id).subscribe({
      next: () => {
        this.snack.open('Started working on ticket', 'Close', { duration: 3000 });
        this.load();
      },
      error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
    });
  }

  resolve() {
    const notes = prompt('Enter resolution notes:');
    if (notes) {
      this.lifecycle.resolve(this.id, notes).subscribe({
        next: () => {
          this.snack.open('Ticket resolved', 'Close', { duration: 3000 });
          this.load();
        },
        error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
      });
    }
  }

  close() {
    if (confirm('Close this ticket?')) {
      this.lifecycle.close(this.id).subscribe({
        next: () => {
          this.snack.open('Ticket closed', 'Close', { duration: 3000 });
          this.load();
        },
        error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
      });
    }
  }

  reopen() {
    const reason = prompt('Reason for reopening:');
    if (reason) {
      this.lifecycle.reopen(this.id, reason).subscribe({
        next: () => {
          this.snack.open('Ticket reopened', 'Close', { duration: 3000 });
          this.load();
        },
        error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
      });
    }
  }

  deleteTicket() {
    if (confirm('Are you sure you want to delete this ticket?')) {
      this.ticketSvc.deleteTicket(this.id).subscribe({
        next: () => {
          this.snack.open('Ticket deleted successfully', 'Close', { duration: 3000 });
          this.goBack();
        },
        error: e => this.snack.open(e.message, 'Close', { duration: 3000 })
      });
    }
  }

  deleteComment(commentId: number, commentUserId: number) {
    if (!this.canDeleteComment(commentUserId)) return;

    if (confirm('Delete this comment?')) {
      this.commentSvc.deleteComment(this.id, commentId).subscribe({
        next: () => {
          this.snack.open('Comment deleted', 'Close', { duration: 3000 });
          this.load();
        }
      });
    }
  }

  // ==================== PERMISSIONS ====================
  canEditTicket(): boolean {
    if (!this.ticket) return false;

    // ❌ EndUser CANNOT edit anymore
    if (this.currentUserRole === 'EndUser') return false;

    // ✅ Only Admin & SupportManager
    return this.currentUserRole === 'Admin' || this.currentUserRole === 'SupportManager';
  }

  canDeleteTicket(): boolean {
    if (!this.ticket) return false;

    if (this.currentUserRole === 'Admin') return true;

    if (this.currentUserRole === 'EndUser') {
      return this.ticket.createdById === this.currentUserId &&
             this.ticket.status === 'Created';
    }

    return false;
  }

  canResolve(): boolean {
    if (!this.ticket) return false;

    if (this.currentUserRole === 'Admin' || this.currentUserRole === 'SupportManager') return true;

    if (this.currentUserRole === 'SupportAgent') {
      return this.ticket.assignedToId === this.currentUserId;
    }

    return false;
  }

  canStartWork(): boolean {
    if (!this.ticket) return false;

    if (this.currentUserRole === 'SupportAgent') {
      return this.ticket.status === 'Assigned' && this.ticket.assignedToId === this.currentUserId;
    }

    if (this.currentUserRole === 'Admin' || this.currentUserRole === 'SupportManager') {
      return this.ticket.status === 'Assigned';
    }

    return false;
  }

  canSeeInternal(): boolean {
    return ['Admin', 'SupportManager', 'SupportAgent'].includes(this.currentUserRole);
  }

  canUpdate(): boolean {
    return ['Admin', 'SupportManager', 'SupportAgent'].includes(this.currentUserRole);
  }

  canDeleteComment(commentUserId: number): boolean {
    return this.currentUserId === commentUserId || this.currentUserRole === 'Admin';
  }

  goBack() {
    this.router.navigate(this.currentUserRole === 'EndUser' ? ['/my-tickets'] : ['/tickets']);
  }
}
