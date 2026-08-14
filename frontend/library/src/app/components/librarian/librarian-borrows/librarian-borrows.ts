import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BorrowService, BorrowDto, FilterBorrowDto } from '../../../services/borrow.service';
import { AuthService } from '../../../services/auth.service';
import { RouterModule, Router } from '@angular/router';
import { BorrowStatus, borrowStatusLabel, isActiveBorrow } from '../../../models/borrow-status.model';
import { extractErrorMessage } from '../../../utils/http-error.util';

@Component({
  selector: 'app-librarian-borrows',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './librarian-borrows.html',
  styleUrl: './librarian-borrows.css'
})
export class LibrarianBorrowsComponent implements OnInit {
  private borrowService = inject(BorrowService);
  private authService = inject(AuthService);
  private router = inject(Router);
  librarianName: string = 'Librarian';
  borrows: BorrowDto[] = [];
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  readonly BorrowStatus = BorrowStatus;
  readonly isActiveBorrow = isActiveBorrow;
  readonly borrowStatusLabel = borrowStatusLabel;

  filter: FilterBorrowDto = {
    pageNumber: 1,
    pageSize: 20,
    id: undefined,
    borrowDate: '',
    status: undefined
  };

  ngOnInit(): void {
    this.loadLibrarianData();
    this.loadBorrows();
  }

  loadLibrarianData(): void {
    this.librarianName = this.authService.getUserName('Librarian');
  }

  loadBorrows(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.borrowService.getAllBorrows(this.filter).subscribe({
      next: (data) => {
        this.borrows = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load borrow records', err);
        this.errorMessage = extractErrorMessage(err, 'Failed to load borrow records.');
        this.isLoading = false;
      }
    });
  }

  onFilterChange(): void {
    this.filter.pageNumber = 1;
    this.loadBorrows();
  }

  clearFilters(): void {
    this.filter = {
      pageNumber: 1,
      pageSize: 20,
      id: undefined,
      borrowDate: '',
      status: undefined
    };
    this.loadBorrows();
  }

  deleteBorrow(id: number): void {
    if (!confirm('Are you sure you want to delete this borrow record?')) return;

    this.borrowService.deleteBorrow(id).subscribe({
      next: () => {
        this.successMessage = 'Borrow record deleted successfully.';
        this.loadBorrows();
        setTimeout(() => this.successMessage = '', 4000);
      },
      error: (err) => {
        console.error('Failed to delete borrow record', err);
        this.errorMessage = extractErrorMessage(err, 'Failed to delete borrow record.');
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
