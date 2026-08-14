import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface StudentDto {
  id: number;
  name: string;
  faculty: string;
  majorName: string;
  email: string;
  phone: string;
}

export interface FilterStudentDto {
  id?: number;
  name?: string;
  faculty?: string;
  majorName?: string;
}

export interface SaveStudentDto {
  name: string;
  faculty: string;
  majorName: string;
  email: string;
  phone: string;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private apiUrl = `${environment.apiUrl}/Students`;

  getAllStudents(filter?: FilterStudentDto): Observable<StudentDto[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.id) params = params.set('Id', filter.id.toString());
      if (filter.name) params = params.set('Name', filter.name);
      if (filter.faculty) params = params.set('Faculty', filter.faculty);
      if (filter.majorName) params = params.set('MajorName', filter.majorName);
    }

    return this.http.get<StudentDto[]>(`${this.apiUrl}/GetAllStudents`, { params });
  }

  // The backend ignores whatever id is sent here and always updates the
  // student record belonging to the authenticated caller (see
  // StudentsController.UpdateStudent, which looks the record up by
  // UserId == the JWT's user id, not by the id parameter). It's still
  // required on the query string for model binding to succeed, so we source
  // it from the caller's own token rather than accepting it as a parameter -
  // that way nothing can pass someone else's id expecting it to be honored.
  updateStudent(dto: SaveStudentDto): Observable<void> {
    const ownId = this.authService.getUserId();
    return this.http.put<void>(`${this.apiUrl}/UpdateStudent?id=${ownId}`, dto);
  }

  deleteStudent(id: number): Observable<void> {
    const params = new HttpParams().set('id', id.toString());
    return this.http.delete<void>(`${this.apiUrl}/DeleteStudent`, { params });
  }
}
