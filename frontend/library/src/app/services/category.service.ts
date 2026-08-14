import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CategoryDto {
  id: number;
  name: string;
  createdAt: string;
}

export interface FilterCategoryDto {
  id?: number;
  name?: string;
}

export interface SaveCategoryDto {
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/Categories`;

  getAllCategories(filter?: FilterCategoryDto): Observable<CategoryDto[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.id) params = params.set('Id', filter.id.toString());
      if (filter.name) params = params.set('Name', filter.name);
    }

    return this.http.get<CategoryDto[]>(`${this.apiUrl}/GetAllCategories`, { params });
  }

  addCategory(dto: SaveCategoryDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/AddCategory`, dto);
  }

  updateCategory(id: number, dto: SaveCategoryDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/UpdateCategory?id=${id}`, dto);
  }

  deleteCategory(id: number): Observable<void> {
    const params = new HttpParams().set('id', id.toString());
    return this.http.delete<void>(`${this.apiUrl}/DeleteCategory`, { params });
  }
}
