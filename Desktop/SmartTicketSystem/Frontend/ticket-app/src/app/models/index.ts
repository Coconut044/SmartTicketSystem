export interface User {
  id: number;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

// Ticket Models
export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  categoryId: number;
  categoryName: string;
  createdById: number;
  createdByName: string;
  assignedToId?: number;
  assignedToName?: string;
  createdAt: Date;
  updatedAt?: Date;
  resolvedAt?: Date;
  closedAt?: Date;
  dueDate?: Date;
  resolutionNotes?: string;
  commentCount: number;
  isOverdue: boolean;
}

export interface TicketDetail extends Ticket {
  comments: Comment[];
  history: TicketHistory[];
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
  categoryId: number;
}

export interface UpdateTicketRequest {
  title?: string;
  description?: string;
  priority?: string;
  categoryId?: number;
  status?: string;
  assignedToId?: number;
  resolutionNotes?: string;
}

// Comment Models
export interface Comment {
createdBy: any;
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  comment: string;
  isInternal: boolean;
  createdAt: Date;
}

export interface AddCommentRequest {
  comment: string;
  isInternal: boolean;
}

// History Model
export interface TicketHistory {
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  action: string;
  oldValue?: string;
  newValue?: string;
  notes?: string;
  createdAt: Date;
}

// Category Models
export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  slaHours?: number;
  createdAt: Date;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  slaHours?: number;
}

// Notification Model
export interface Notification {
  id: number;
  userId: number;
  ticketId?: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
}

// Dashboard Models
export interface DashboardStats {
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

// Report Models
export interface SlaComplianceReport {
  totalTickets: number;
  withinSla: number;
  breachedSla: number;
  complianceRate: number;
  breaches: SlaBreachDetail[];
}

export interface SlaBreachDetail {
  ticketId: number;
  title: string;
  priority: string;
  createdAt: Date;
  dueDate: Date;
  hoursOverdue: number;
}

export interface AgentWorkloadReport {
  agentWorkloads: AgentWorkload[];
  averageWorkload: number;
}

export interface AgentWorkload {
  agentId: number;
  agentName: string;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  totalAssigned: number;
  averageResolutionTimeHours: number;
}

// ✅ ADDED: AgentPerformance interface
export interface AgentPerformance {
  agentId: number;
  agentName: string;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  totalAssigned: number;
  averageResolutionTimeHours: number;
}

export interface ResolutionTimeReport {
  averageResolutionTimeHours: number;
  medianResolutionTimeHours: number;
  byPriority: { [key: string]: number };
  byCategory: { [key: string]: number };
}

// API Response Models
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// Constants and Enums
export const TICKET_STATUSES = [
  { value: 'Created', label: 'Created' },
  { value: 'Assigned', label: 'Assigned' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Resolved', label: 'Resolved' },
  { value: 'Closed', label: 'Closed' },
  { value: 'Cancelled', label: 'Cancelled' }
];

export const TICKET_PRIORITIES = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' }
];

export const USER_ROLES = [
  { value: 'EndUser', label: 'End User' },
  { value: 'SupportAgent', label: 'Support Agent' },
  { value: 'SupportManager', label: 'Support Manager' },
  { value: 'Admin', label: 'Administrator' }
];
