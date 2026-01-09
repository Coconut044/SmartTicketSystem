import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models';


interface ReportByStatusDto {
  statusCounts: { [key: string]: number };
  statusPercentages: { [key: string]: number };
  totalTickets: number;
}

interface ReportByPriorityDto {
  priorityCounts: { [key: string]: number };
  priorityPercentages: { [key: string]: number };
  totalTickets: number;
}

interface ReportByCategoryDto {
  categoryCounts: { [key: string]: number };
  categoryPercentages: { [key: string]: number };
  totalTickets: number;
}

interface SlaComplianceReportDto {
  totalTickets: number;
  withinSla: number;
  breachedSla: number;
  complianceRate: number;
  breaches: SlaBreachDetail[];
}

interface SlaBreachDetail {
  ticketId: number;
  title: string;
  priority: string;
  createdAt: Date;
  dueDate: Date;
  hoursOverdue: number;
}

interface AgentWorkloadReportDto {
  agentWorkloads: AgentWorkload[];
  averageWorkload: number;
}

interface AgentWorkload {
  agentId: number;
  agentName: string;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  totalAssigned: number;
  averageResolutionTimeHours: number;
}

interface ResolutionTimeReportDto {
  averageResolutionTimeHours: number;
  medianResolutionTimeHours: number;
  byPriority: { [key: string]: number };
  byCategory: { [key: string]: number };
}

interface TicketTrendReportDto {
  startDate: Date;
  endDate: Date;
  dailyCounts: DailyTicketCount[];
  totalCreated: number;
  totalResolved: number;
}

interface DailyTicketCount {
  date: Date;
  created: number;
  resolved: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportingService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}


  getByStatus(): Observable<ApiResponse<ReportByStatusDto>> {
    return this.http.get<ApiResponse<ReportByStatusDto>>(`${this.apiUrl}/by-status`);
  }

  getByPriority(): Observable<ApiResponse<ReportByPriorityDto>> {
    return this.http.get<ApiResponse<ReportByPriorityDto>>(`${this.apiUrl}/by-priority`);
  }

  getByCategory(): Observable<ApiResponse<ReportByCategoryDto>> {
    return this.http.get<ApiResponse<ReportByCategoryDto>>(`${this.apiUrl}/by-category`);
  }

  getSlaCompliance(): Observable<ApiResponse<SlaComplianceReportDto>> {
    return this.http.get<ApiResponse<SlaComplianceReportDto>>(`${this.apiUrl}/sla-compliance`);
  }

  getAgentWorkload(): Observable<ApiResponse<AgentWorkloadReportDto>> {
    return this.http.get<ApiResponse<AgentWorkloadReportDto>>(`${this.apiUrl}/agent-workload`);
  }

  getResolutionTime(): Observable<ApiResponse<ResolutionTimeReportDto>> {
    return this.http.get<ApiResponse<ResolutionTimeReportDto>>(`${this.apiUrl}/resolution-time`);
  }

  getTicketTrend(startDate?: Date, endDate?: Date): Observable<ApiResponse<TicketTrendReportDto>> {
    let url = `${this.apiUrl}/trend`;
    const params: string[] = [];
    
    if (startDate) {
      params.push(`startDate=${startDate.toISOString()}`);
    }
    if (endDate) {
      params.push(`endDate=${endDate.toISOString()}`);
    }
    
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }
    
    return this.http.get<ApiResponse<TicketTrendReportDto>>(url);
  }
}