import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { TicketService } from '../../core/services/ticket.service';
import { AuthService } from '../../core/services/auth.service';
import { Ticket } from '../../models';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule
  ],
  templateUrl: './ticket-list.page.html',
  styleUrls: ['./ticket-list.page.scss']
})
export class TicketListPage implements OnInit {
  tickets: Ticket[] = [];
  loading = true;

  displayedColumns = ['id', 'title', 'status', 'priority', 'createdAt'];

  isAgent = false;
  isManagerOrAdmin = false;

  constructor(
    private ticketService: TicketService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const role = this.auth.getUserRole();
    this.isAgent = role === 'SupportAgent';
    this.isManagerOrAdmin = role === 'SupportManager' || role === 'Admin';

    this.loadTickets();
  }

  loadTickets() {
    this.loading = true;
    this.ticketService.getTickets().subscribe({
      next: r => {
        if (r.success && r.data) {
          this.tickets = r.data.items;
        }
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }
}
