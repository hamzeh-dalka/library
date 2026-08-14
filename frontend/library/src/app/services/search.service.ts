import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// واجهة نتيجة البحث الذكي المتوافقة مع مخرجات الـ Backend
export interface SmartSearchResultDto {
  id: number;
  title: string;
  author: string;
  similarity: number;
}

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/Search`;

  /**
   * تنفيذ البحث الذكي (Smart Search) باستخدام الذكاء الاصطناعي
   * @param query نص البحث المدخل من المستخدم
   */
  smartSearch(query: string): Observable<SmartSearchResultDto[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<SmartSearchResultDto[]>(`${this.apiUrl}/smart-search`, { params });
  }
}
