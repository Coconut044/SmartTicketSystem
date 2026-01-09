import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LifecycleService {

  private apiUrl = `${environment.apiUrl}/ticketlifecycle`;

  constructor(private http: HttpClient) {}

 
  startWork(id: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/start`,
      {}
    );
  }

 
  resolve(id: number, notes: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/resolve`,
      JSON.stringify(notes),
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      }
    );
  }

  
  close(id: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/close`,
      {}
    );
  }

  
  reopen(id: number, reason: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/reopen`,
      JSON.stringify(reason),
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      }
    );
  }

  
  cancel(id: number, reason: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${id}/cancel`,
      JSON.stringify(reason),
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      }
    );
  }
}
