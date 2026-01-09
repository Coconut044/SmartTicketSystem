import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Comment, AddCommentRequest, ApiResponse } from '../../models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private apiUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  getComments(ticketId: number): Observable<ApiResponse<Comment[]>> {
    return this.http.get<ApiResponse<Comment[]>>(`${this.apiUrl}/${ticketId}/comments`);
  }

  addComment(ticketId: number, req: AddCommentRequest): Observable<ApiResponse<Comment>> {
    return this.http.post<ApiResponse<Comment>>(`${this.apiUrl}/${ticketId}/comments`, req);
  }
deleteComment(ticketId: number, commentId: number): Observable<ApiResponse<boolean>> {
  return this.http.delete<ApiResponse<boolean>>(
    `${this.apiUrl}/tickets/${ticketId}/comments/${commentId}`
  );
}
}