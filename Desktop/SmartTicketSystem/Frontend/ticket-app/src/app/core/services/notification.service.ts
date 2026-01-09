import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification, ApiResponse } from '../../models';

@Injectable({ providedIn: 'root' })
export class NotificationService {

  private apiUrl = `${environment.apiUrl}/notifications`;

  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  getNotifications(unreadOnly = false): Observable<ApiResponse<Notification[]>> {
    const params = new HttpParams().set('unreadOnly', unreadOnly);
    return this.http.get<ApiResponse<Notification[]>>(this.apiUrl, { params });
  }

  getUnreadCount(): Observable<ApiResponse<number>> {
    return this.http
      .get<ApiResponse<number>>(`${this.apiUrl}/unread-count`)
      .pipe(
        tap(res => {
          if (res.success && res.data !== undefined) {
            this.unreadCountSubject.next(res.data);
          }
        })
      );
  }

  markAsRead(id: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => this.getUnreadCount().subscribe())
    );
  }

  markAllAsRead(): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/mark-all-read`, {}).pipe(
      tap(() => this.getUnreadCount().subscribe())
    );
  }
}
