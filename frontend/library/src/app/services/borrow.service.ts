import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BookDto } from './book.service';
import { BorrowStatus } from '../models/borrow-status.model';

// Re-exported so existing `import { BookDto } from './borrow.service'` call
// sites keep working - the canonical definition lives in book.service.ts.
export type { BookDto };

export interface BorrowDto {
  id: number;
  borrowDate: string;
  dueDate: string;
  returnDate?: string;
  status: BorrowStatus;
  studentId: number;
  // Not populated by GetBorrowsForStudent (BorrowsController.cs) - only
  // GetAllBorrows (the librarian-facing endpoint) includes it.
  studentName?: string;
  bookId: number;
  bookTitle: string;
  bookAuthor: string;
}

export interface FilterBorrowDto {
  id?: number;
  borrowDate?: string;
  status?: BorrowStatus;
  pageNumber: number;
  pageSize: number;
}

export interface SaveBorrowDto {
  borrowDate: string;
  dueDate: string;
  returnDate?: string;
  bookId: number;
}

export interface ExtendBorrowDto {
  newDueDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class BorrowService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/Borrows`;


  getAllBorrows(filter: FilterBorrowDto): Observable<BorrowDto[]> {
    let params = new HttpParams()
      .set('PageNumber', filter.pageNumber.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.id) params = params.set('Id', filter.id.toString());
    if (filter.borrowDate) params = params.set('BorrowDate', filter.borrowDate);
    if (filter.status !== undefined && filter.status !== null) {
      params = params.set('Status', filter.status.toString());
    }

    return this.http.get<BorrowDto[]>(`${this.apiUrl}/GetAllBorrows`, { params });
  }

  // Returns the current student's own borrow records (id, dates, status,
  // book info) - see BorrowsController.GetBorrowsForStudent. StudentName is
  // the one BorrowDto field it omits (see BorrowDto.studentName above).
  getBorrowsForStudent(): Observable<BorrowDto[]> {
    return this.http.get<BorrowDto[]>(`${this.apiUrl}/GetBorrowsForStudent`);
  }

  createBorrow(dto: SaveBorrowDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/CreateBorrow`, dto);
  }

  returnBook(id: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/ReturnBook/${id}`, {});
  }

  extendDueDate(id: number, dto: ExtendBorrowDto): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/ExtendDueDate/${id}`, dto);
  }

  deleteBorrow(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/DeleteBorrow/${id}`);
  }
}
