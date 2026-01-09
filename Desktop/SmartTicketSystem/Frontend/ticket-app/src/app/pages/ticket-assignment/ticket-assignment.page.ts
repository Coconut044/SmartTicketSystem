import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { AssignmentService } from '../../core/services/assignment.service';
import { UserService } from '../../core/services/user.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../shared/components/priority-badge/priority-badge.component';
import { Ticket, User } from '../../models';

@Component({
  selector: 'app-ticket-assignment',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatChipsModule,
    LoadingComponent,
    StatusBadgeComponent,
    PriorityBadgeComponent
  ],
  templateUrl: './ticket-assignment.page.html',
  styleUrls: ['./ticket-assignment.page.scss']
})
export class TicketAssignmentPage implements OnInit {
  unassignedTickets: Ticket[] = [];
  supportAgents: User[] = [];
  agentWorkload: Map<number, number> = new Map();
  columns = ['id', 'title', 'priority', 'category', 'createdBy', 'createdAt', 'assign'];
  loading = true;
  selectedAgent: { [ticketId: number]: number } = {};

  constructor(
    private assignmentService: AssignmentService,
    private userService: UserService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;

    // Load unassigned tickets
    this.assignmentService.getUnassignedTickets().subscribe({
      next: r => {
        if (r.success && r.data) {
          this.unassignedTickets = r.data;
        }
      },
      error: err => {
        console.error('Failed to load unassigned tickets', err);
        this.snackBar.open('Failed to load tickets', 'Close', { duration: 3000 });
      }
    });

    // Load support agents
    this.userService.getUsersByRole('SupportAgent').subscribe({
      next: r => {
        if (r.success && r.data) {
          this.supportAgents = r.data;
        }
      },
      error: err => {
        console.error('Failed to load agents', err);
        this.snackBar.open('Failed to load agents', 'Close', { duration: 3000 });
      }
    });

    // Load agent workload
    this.assignmentService.getAgentWorkload().subscribe({
      next: r => {
        if (r.success && r.data) {
          // Convert object to Map
          this.agentWorkload = new Map(
            Object.entries(r.data).map(([k, v]) => [Number(k), v as number])
          );
        }
        this.loading = false;
      },
      error: err => {
        console.error('Failed to load workload', err);
        this.loading = false;
      }
    });
  }

  assignTicket(ticketId: number) {
    const agentId = this.selectedAgent[ticketId];

    if (!agentId) {
      this.snackBar.open('Please select an agent', 'Close', { duration: 3000 });
      return;
    }

    // ✅ CORRECT - matches your backend endpoint
    this.assignmentService.assignTicketManually(ticketId, agentId).subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open('Ticket assigned successfully', 'Close', { duration: 3000 });
          // Remove from unassigned list
          this.unassignedTickets = this.unassignedTickets.filter(t => t.id !== ticketId);
          // Refresh workload
          this.loadData();
        }
      },
      error: err => {
        console.error('Assignment failed:', err);
        this.snackBar.open('Failed to assign ticket', 'Close', { duration: 3000 });
      }
    });
  }

  autoAssignTicket(ticketId: number) {
    // ✅ CORRECT - matches your backend endpoint
    this.assignmentService.assignTicketAutomatically(ticketId).subscribe({
      next: r => {
        if (r.success) {
          this.snackBar.open('Ticket auto-assigned successfully', 'Close', { duration: 3000 });
          // Remove from unassigned list
          this.unassignedTickets = this.unassignedTickets.filter(t => t.id !== ticketId);
          // Refresh workload
          this.loadData();
        }
      },
      error: err => {
        console.error('Auto-assignment failed:', err);
        this.snackBar.open('Failed to auto-assign ticket', 'Close', { duration: 3000 });
      }
    });
  }

  getAgentWorkload(agentId: number): number {
    return this.agentWorkload.get(agentId) || 0;
  }

  getAgentWithWorkload(agent: User): string {
    const workload = this.getAgentWorkload(agent.id);
    return `${agent.fullName} (${workload} tickets)`;
  }

  getWorkloadColor(workload: number): string {
    if (workload === 0) return '#4caf50'; // Green - Available
    if (workload < 5) return '#8bc34a';   // Light Green
    if (workload < 10) return '#ffc107';  // Yellow
    if (workload < 15) return '#ff9800';  // Orange
    return '#f44336';                      // Red - Overloaded
  }

  getWorkloadStatus(workload: number): string {
    if (workload === 0) return 'Available';
    if (workload < 5) return 'Light';
    if (workload < 10) return 'Moderate';
    if (workload < 15) return 'Heavy';
    return 'Overloaded';
  }
}