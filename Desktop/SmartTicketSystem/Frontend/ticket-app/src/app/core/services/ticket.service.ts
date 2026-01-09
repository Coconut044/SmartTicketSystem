import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Ticket,
  TicketDetail,
  CreateTicketRequest,
  UpdateTicketRequest,
  ApiResponse,
  PagedResult
} from '../../models';

@Injectable({ providedIn: 'root' })
export class TicketService {

  private apiUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  getTickets(
    page = 1,
    size = 10,
    status?: string,
    priority?: string,
    categoryId?: number,
    assignedToId?: number,
    createdById?: number
  ): Observable<ApiResponse<PagedResult<Ticket>>> {

    let params = new HttpParams()
      .set('pageNumber', page)
      .set('pageSize', size);

    if (status) params = params.set('status', status);
    if (priority) params = params.set('priority', priority);
    if (categoryId) params = params.set('categoryId', categoryId);
    if (assignedToId) params = params.set('assignedToId', assignedToId);
    if (createdById) params = params.set('createdById', createdById);

    return this.http.get<ApiResponse<PagedResult<Ticket>>>(this.apiUrl, { params });
  }

  getTicketById(id: number): Observable<ApiResponse<TicketDetail>> {
    return this.http.get<ApiResponse<TicketDetail>>(`${this.apiUrl}/${id}`);
  }

  createTicket(req: CreateTicketRequest): Observable<ApiResponse<Ticket>> {
    return this.http.post<ApiResponse<Ticket>>(this.apiUrl, req);
  }

  updateTicket(id: number, req: UpdateTicketRequest): Observable<ApiResponse<Ticket>> {
    return this.http.put<ApiResponse<Ticket>>(`${this.apiUrl}/${id}`, req);
  }

  assignTicket(ticketId: number, userId: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(
      `${this.apiUrl}/${ticketId}/assign`,
      { userId }
    );
  }

  updateTicketStatus(ticketId: number, status: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.apiUrl}/${ticketId}/status`,
      { status }
    );
  }

  deleteTicket(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
