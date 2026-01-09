import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { ReportingService } from '../../core/services/reporting.service';
import { AuthService } from '../../core/services/auth.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';

interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  overdueTickets: number;
  ticketsByPriority: { [key: string]: number };
  ticketsByCategory: { [key: string]: number };
  ticketsByStatus: { [key: string]: number };
  averageResolutionTimeHours: number;
}

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
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatGridListModule,
    LoadingComponent
  ],
  templateUrl: './reports.page.html',
  styleUrls: ['./reports.page.scss']
})
export class ReportsPage implements OnInit {
  loading = true;
  stats: DashboardStats | null = null;
  agentPerformance: AgentPerformance[] = [];
  priorityKeys: string[] = [];
  categoryKeys: string[] = [];
  statusKeys: string[] = [];

  constructor(
    private reporting: ReportingService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    this.loadStats();
    this.loadAgentPerformance();
  }

  loadStats() {
    this.loading = true;

    // Initialize stats object
    this.stats = {
      totalTickets: 0,
      openTickets: 0,
      inProgressTickets: 0,
      resolvedTickets: 0,
      closedTickets: 0,
      overdueTickets: 0,
      ticketsByPriority: {},
      ticketsByCategory: {},
      ticketsByStatus: {},
      averageResolutionTimeHours: 0
    };

    let completedRequests = 0;
    const totalRequests = 4; // status, priority, category, resolution time

    const checkComplete = () => {
      completedRequests++;
      if (completedRequests === totalRequests) {
        this.loading = false;
      }
    };

    // Load status report
    this.reporting.getByStatus().subscribe({
      next: r => {
        if (r.success && r.data && this.stats) {
          this.stats.ticketsByStatus = r.data.statusCounts;
          this.stats.totalTickets = r.data.totalTickets;
          this.statusKeys = Object.keys(r.data.statusCounts);

          // Calculate derived stats
          this.stats.openTickets =
            (r.data.statusCounts['Created'] || 0) +
            (r.data.statusCounts['Assigned'] || 0);
          this.stats.inProgressTickets = r.data.statusCounts['InProgress'] || 0;
          this.stats.resolvedTickets = r.data.statusCounts['Resolved'] || 0;
          this.stats.closedTickets = r.data.statusCounts['Closed'] || 0;
          // overdueTickets calculated elsewhere if needed
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // Load priority report
    this.reporting.getByPriority().subscribe({
      next: r => {
        if (r.success && r.data && this.stats) {
          this.stats.ticketsByPriority = r.data.priorityCounts;
          this.priorityKeys = Object.keys(r.data.priorityCounts);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // Load category report
    this.reporting.getByCategory().subscribe({
      next: r => {
        if (r.success && r.data && this.stats) {
          this.stats.ticketsByCategory = r.data.categoryCounts;
          this.categoryKeys = Object.keys(r.data.categoryCounts);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // Load resolution time
    this.reporting.getResolutionTime().subscribe({
      next: r => {
        if (r.success && r.data && this.stats) {
          this.stats.averageResolutionTimeHours = r.data.averageResolutionTimeHours;
        }
        checkComplete();
      },
      error: () => checkComplete()
    });
  }

  loadAgentPerformance() {
    const currentUser = this.auth.getCurrentUser();
    
    this.reporting.getAgentWorkload().subscribe({
      next: r => {
        if (r.success && r.data) {
          // Filter to show only the logged-in agent's workload
          if (currentUser) {
            this.agentPerformance = r.data.agentWorkloads.filter(
              (agent: AgentPerformance) => agent.agentId === currentUser.id
            );
          }
        }
      },
      error: err => console.error('Failed to load agent performance', err)
    });
  }

  refresh() {
    this.loadStats();
    this.loadAgentPerformance();
  }

  exportReport() {
    console.log('Export report');
  }

  getPercentage(value: number, total: number): number {
    return total > 0 ? Math.round((value / total) * 100) : 0;
  }
}