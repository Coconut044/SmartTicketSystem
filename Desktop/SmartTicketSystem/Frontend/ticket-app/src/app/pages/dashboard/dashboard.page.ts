import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../shared/components/priority-badge/priority-badge.component';
import { DashboardStats } from '../../models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, LoadingComponent, StatusBadgeComponent, PriorityBadgeComponent],
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss']
})
export class DashboardPage implements OnInit {
  stats: DashboardStats | null = null;
  loading = true;

  constructor(private dash: DashboardService, public auth: AuthService) {}

  ngOnInit() {
    this.dash.getStats().subscribe({
      next: r => {
        if (r.success && r.data) this.stats = r.data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  getStatusData() {
    return this.stats ? Object.entries(this.stats.ticketsByStatus).map(([status, count]) => ({ status, count })) : [];
  }

  getPriorityData() {
    return this.stats ? Object.entries(this.stats.ticketsByPriority).map(([priority, count]) => ({ priority, count })) : [];
  }

  getPercentage(value: number, total: number) {
    return total > 0 ? (value / total) * 100 : 0;
  }
}