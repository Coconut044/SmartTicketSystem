import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category, CreateCategoryRequest, ApiResponse } from '../../models';

@Injectable({ providedIn: 'root' })
export class CategoryService {

  private apiUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}


  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl);
  }

 
  createCategory(req: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, req);
  }

  
  updateCategory(
    id: number,
    data: Partial<CreateCategoryRequest>
  ): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(
      `${this.apiUrl}/${id}`,
      data
    );
  }


  deleteCategory(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
