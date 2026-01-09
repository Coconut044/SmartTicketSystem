import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Ticket } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AssignmentService {
  private apiUrl = `${environment.apiUrl}/assignment`;

  constructor(private http: HttpClient) {}

  // Get all unassigned tickets
  getUnassignedTickets(): Observable<ApiResponse<Ticket[]>> {
    return this.http.get<ApiResponse<Ticket[]>>(`${this.apiUrl}/unassigned`);
  }

  // Manually assign ticket to specific agent ✅ FIXED ENDPOINT
  assignTicketManually(
    ticketId: number,
    agentId: number
  ): Observable<ApiResponse<Ticket>> {
    return this.http.post<ApiResponse<Ticket>>(
      `${this.apiUrl}/${ticketId}/assign/${agentId}`,
      {}
    );
  }

  // Auto-assign ticket based on workload ✅ FIXED ENDPOINT
  assignTicketAutomatically(
    ticketId: number
  ): Observable<ApiResponse<Ticket>> {
    return this.http.post<ApiResponse<Ticket>>(
      `${this.apiUrl}/${ticketId}/auto-assign`,
      {}
    );
  }

  // Get agent workload (tickets assigned per agent)
  getAgentWorkload(): Observable<ApiResponse<{ [agentId: number]: number }>> {
    return this.http.get<ApiResponse<{ [agentId: number]: number }>>(
      `${this.apiUrl}/workload`
    );
  }

  // Escalate overdue tickets
  escalateOverdueTickets(): Observable<ApiResponse<Ticket[]>> {
    return this.http.post<ApiResponse<Ticket[]>>(
      `${this.apiUrl}/escalate-overdue`,
      {}
    );
  }
}
