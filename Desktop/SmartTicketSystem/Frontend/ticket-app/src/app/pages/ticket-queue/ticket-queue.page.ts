import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TicketService } from '../../core/services/ticket.service';
import { CategoryService } from '../../core/services/category.service';
import { UserService } from '../../core/services/user.service';
import { LifecycleService } from '../../core/services/lifecycle.service';
import { ReportingService } from '../../core/services/reporting.service';
import { AuthService } from '../../core/services/auth.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../shared/components/priority-badge/priority-badge.component';
import { Ticket, Category, User, TICKET_STATUSES, TICKET_PRIORITIES } from '../../models';

interface AgentPerformance {
  agentId: number;
  agentName: string;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  totalAssigned: number;
  averageResolutionTimeHours: number;
}

@Component({
  selector: 'app-ticket-queue',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    MatCardModule, 
    MatButtonModule, 
    MatFormFieldModule, 
    MatSelectModule, 
    MatTableModule, 
    MatPaginatorModule, 
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatSnackBarModule,
    LoadingComponent, 
    StatusBadgeComponent, 
    PriorityBadgeComponent
  ],
  templateUrl: './ticket-queue.page.html',
  styleUrls: ['./ticket-queue.page.scss']
})
export class TicketQueuePage implements OnInit {
  tickets: Ticket[] = [];
  categories: Category[] = [];
  agents: User[] = [];
  statuses = TICKET_STATUSES;
  priorities = TICKET_PRIORITIES;
  
  loading = true;
  page = 1;
  size = 10;
  total = 0;
  
  filterStatus = '';
  filterPriority = '';
  filterCategory: number | '' = '';
  filterAssignment = ''; // 'unassigned' or 'assigned' for managers
  
  currentUserId: number = 0;
  currentUserRole: string = '';
  
  myWorkload: AgentPerformance | null = null;

  // Column definitions
  get displayedColumns(): string[] {
    if (this.isManager()) {
      return ['id', 'title', 'status', 'priority', 'category', 'createdBy', 'assignedTo', 'dueDate', 'actions'];
    } else {
      return ['id', 'title', 'status', 'priority', 'category', 'createdBy', 'dueDate', 'actions'];
    }
  }

  constructor(
    private ticket: TicketService,
    private cat: CategoryService,
    private userService: UserService,
    private lifecycle: LifecycleService,
    private reporting: ReportingService,
    private auth: AuthService,
    private snack: MatSnackBar
  ) {}

  ngOnInit() {
    const user = this.auth.getCurrentUser();
    if (user) {
      this.currentUserId = user.id;
      this.currentUserRole = user.role;
    }
    
    this.loadCategories();
    this.loadAgents();
    this.loadMyWorkload();
    this.load();
  }

  loadCategories() {
    this.cat.getCategories().subscribe({
      next: r => {
        if (r.success && r.data) {
          this.categories = r.data;
        }
      }
    });
  }

  loadAgents() {
    this.userService.getUsersByRole('SupportAgent').subscribe({
      next: r => {
        if (r.success && r.data) {
          this.agents = r.data;
        }
      },
      error: err => console.error('Failed to load agents', err)
    });
  }

  loadMyWorkload() {
    // Only load workload for support agents
    if (!this.isManager()) {
      console.log('Loading workload for agent:', this.currentUserId);
      this.reporting.getAgentWorkload().subscribe({
        next: r => {
          console.log('Workload response:', r);
          if (r.success && r.data) {
            // Filter to get only current agent's workload
            const myData = r.data.agentWorkloads.find(
              (agent: AgentPerformance) => agent.agentId === this.currentUserId
            );
            console.log('My workload data:', myData);
            if (myData) {
              this.myWorkload = myData;
            }
          }
        },
        error: err => console.error('Failed to load workload', err)
      });
    } else {
      console.log('User is manager, not loading workload');
    }
  }

  load() {
    this.loading = true;
    const catId = this.filterCategory ? Number(this.filterCategory) : undefined;
    
    let assignedToId: number | undefined = undefined;

    // ✅ MANAGER: Can see all tickets or filter by assignment
    if (this.isManager()) {
      if (this.filterAssignment === 'unassigned') {
        // Special handling: we'll filter after getting results
        // OR modify backend to support this filter
      } else if (this.filterAssignment === 'assigned') {
        
      }
      
    } 
    
    else {
      assignedToId = this.currentUserId;
    }
    
    this.ticket.getTickets(
      this.page, 
      this.size, 
      this.filterStatus || undefined, 
      this.filterPriority || undefined, 
      catId, 
      assignedToId,
      undefined
    ).subscribe({
      next: r => {
        if (r.success && r.data) {
          let tickets = r.data.items;

          // Manual filter for unassigned (if backend doesn't support it)
          if (this.isManager() && this.filterAssignment === 'unassigned') {
            tickets = tickets.filter(t => !t.assignedToId);
          } else if (this.isManager() && this.filterAssignment === 'assigned') {
            tickets = tickets.filter(t => t.assignedToId);
          }

          this.tickets = tickets;
          this.total = r.data.totalCount;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onPage(e: PageEvent) {
    this.page = e.pageIndex + 1;
    this.size = e.pageSize;
    this.load();
  }

  filter() {
    this.page = 1;
    this.load();
  }

  clear() {
    this.filterStatus = '';
    this.filterPriority = '';
    this.filterCategory = '';
    this.filterAssignment = '';
    this.filter();
  }

  
  assignTicket(ticketId: number, agentId: number) {
    if (!this.isManager()) {
      this.snack.open('Only managers can assign tickets', 'Close', { duration: 3000 });
      return;
    }

    this.ticket.assignTicket(ticketId, agentId).subscribe({
      next: r => {
        if (r.success) {
          this.snack.open('Ticket assigned successfully', 'Close', { duration: 3000 });
          this.load();
        }
      },
      error: err => this.snack.open('Failed to assign ticket', 'Close', { duration: 3000 })
    });
  }

  
  startWork(ticketId: number) {
    this.lifecycle.startWork(ticketId).subscribe({
      next: () => {
        this.snack.open('Work started - Status updated to In Progress', 'Close', { duration: 3000 });
        this.load();
        this.loadMyWorkload(); // Refresh workload after starting work
      },
      error: err => this.snack.open('Failed to start work', 'Close', { duration: 3000 })
    });
  }

  
  isManager(): boolean {
    return ['Admin', 'SupportManager'].includes(this.currentUserRole);
  }
}