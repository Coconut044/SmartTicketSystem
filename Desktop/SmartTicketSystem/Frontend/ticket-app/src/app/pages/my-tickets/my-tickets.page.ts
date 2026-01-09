import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { TicketService } from '../../core/services/ticket.service';
import { AuthService } from '../../core/services/auth.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../shared/components/priority-badge/priority-badge.component';
import { Ticket } from '../../models';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatChipsModule,
    LoadingComponent,
    StatusBadgeComponent,
    PriorityBadgeComponent
  ],
  templateUrl: './my-tickets.page.html',
  styleUrls: ['./my-tickets.page.scss']
})
export class MyTicketsPage implements OnInit {
  tickets: Ticket[] = [];
  loading = true;

  displayedColumns = ['id', 'title', 'status', 'priority', 'assignedTo', 'createdAt', 'actions'];

  constructor(
    private ticketService: TicketService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    this.loadMyTickets();
  }

  loadMyTickets() {
    this.loading = true;
    
    // Get current user's tickets only
    const user = this.auth.getCurrentUser();
    if (!user) {
      this.loading = false;
      return;
    }

    // Filter by created by current user
    this.ticketService.getTickets(1, 100, undefined, undefined, undefined, undefined, user.id).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.tickets = res.data.items;
        }
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Created': return 'fiber_new';
      case 'Assigned': return 'assignment_ind';
      case 'InProgress': return 'hourglass_empty';
      case 'Resolved': return 'check_circle';
      case 'Closed': return 'lock';
      case 'Cancelled': return 'cancel';
      default: return 'help';
    }
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Created': return '#2196f3';
      case 'Assigned': return '#ff9800';
      case 'InProgress': return '#ffc107';
      case 'Resolved': return '#4caf50';
      case 'Closed': return '#9e9e9e';
      case 'Cancelled': return '#f44336';
      default: return '#000';
    }
  }

  getStatusMessage(ticket: Ticket): string {
    switch (ticket.status) {
      case 'Created':
        return 'Your ticket has been created and is waiting to be assigned to a support agent.';
      case 'Assigned':
        return `Your ticket has been assigned to ${ticket.assignedToName || 'a support agent'}.`;
      case 'InProgress':
        return `${ticket.assignedToName || 'A support agent'} is currently working on your ticket.`;
      case 'Resolved':
        return 'Your ticket has been resolved! Please review the resolution.';
      case 'Closed':
        return 'This ticket has been closed.';
      case 'Cancelled':
        return 'This ticket has been cancelled.';
      default:
        return 'Status unknown';
    }
  }
}