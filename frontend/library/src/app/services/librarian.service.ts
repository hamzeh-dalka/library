import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface LibrarianDto {
  id: number;
  name: string;
  email: string;
  phone: string;
}

export interface FilterLibrarianDto {
  id?: number;
  name?: string;
}

export interface SaveLibrarianDto {
  name: string;
  email: string;
  phone: string;
  password: string;
  userName: string;
}

export interface UpdateLibrarianDto {
  name: string;
  email: string;
  phone: string;
}

@Injectable({
  providedIn: 'root'
})
export class LibrarianService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private apiUrl = `${environment.apiUrl}/Librarians`;

  getAllLibrarians(filter?: FilterLibrarianDto): Observable<LibrarianDto[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.id) params = params.set('Id', filter.id.toString());
      if (filter.name) params = params.set('Name', filter.name);
    }

    return this.http.get<LibrarianDto[]>(`${this.apiUrl}/GetAllLibrarians`, { params });
  }

  // The backend ignores whatever id is sent here and always updates the
  // librarian record belonging to the authenticated caller (see
  // LibrariansController.UpdateLibrarian, which looks the record up by
  // UserId == the JWT's user id, not by the id parameter). It's still
  // required on the query string for model binding to succeed, so we source
  // it from the caller's own token rather than accepting it as a parameter -
  // that way nothing can pass someone else's id expecting it to be honored.
  updateLibrarian(dto: UpdateLibrarianDto): Observable<void> {
    const ownId = this.authService.getUserId();
    return this.http.put<void>(`${this.apiUrl}/UpdateLibrarian?id=${ownId}`, dto);
  }

  deleteLibrarian(id: number): Observable<void> {
    const params = new HttpParams().set('id', id.toString());
    return this.http.delete<void>(`${this.apiUrl}/DeleteLibrarian`, { params });
  }
}
