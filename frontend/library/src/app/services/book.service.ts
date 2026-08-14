import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BookDto {
  id: number;
  title: string;
  author: string;
  totalCopies: number;
  availableCopies: number;
  publishedYear: number;
  categoryId: number;
  categoryName?: string;
  createdAt: string;
}

export interface FilterBookDto {
  id?: number;
  author?: string;
  title?: string;
  publishedYear?: number;
  pageNumber: number;
  pageSize: number;
}

export interface SaveBookDto {
  title: string;
  author: string;
  totalCopies: number;
  availableCopies?: number;
  publishedYear: number;
  categoryId: number;
}

@Injectable({
  providedIn: 'root'
})
export class BookService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/Books`;

  getAllBooks(filter: FilterBookDto): Observable<BookDto[]> {
    let params = new HttpParams()
      .set('PageNumber', filter.pageNumber.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.id) params = params.set('Id', filter.id.toString());
    if (filter.author) params = params.set('Author', filter.author);
    if (filter.title) params = params.set('Title', filter.title);
    if (filter.publishedYear) params = params.set('PublishedYear', filter.publishedYear.toString());

    return this.http.get<BookDto[]>(`${this.apiUrl}/GetAllBooks`, { params });
  }

  addBook(dto: SaveBookDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/AddBook`, dto);
  }

  updateBook(id: number, dto: SaveBookDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/UpdateBook/${id}`, dto);
  }

  getRecommendedBooks(): Observable<BookDto[]> {
    return this.http.get<BookDto[]>(`${this.apiUrl}/GetRecommendedBooks`);
  }

  deleteBook(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/DeleteBook/${id}`);
  }
}
