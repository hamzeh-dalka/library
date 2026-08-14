import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BorrowService, BorrowDto } from '../../../services/borrow.service';
import { AuthService } from '../../../services/auth.service';
import { Router , RouterModule} from '@angular/router';
import { extractErrorMessage } from '../../../utils/http-error.util';
import { BorrowStatus, borrowStatusLabel, isActiveBorrow } from '../../../models/borrow-status.model';

@Component({
  selector: 'app-student-borrows',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-borrows.html',
  styleUrl: './student-borrows.css'
})
export class StudentBorrowsComponent implements OnInit {
  private borrowService = inject(BorrowService);
  private authService = inject(AuthService);
  private router = inject(Router);

  studentName: string = 'Student';
  studentEmail: string = '';

  readonly borrowStatusLabel = borrowStatusLabel;
  readonly isActiveBorrow = isActiveBorrow;
  readonly BorrowStatus = BorrowStatus;

  borrows: BorrowDto[] = [];
  isLoading = false;
  errorMessage = '';

  // Tracks which borrow's Return button is mid-request, so only that row
  // shows a spinner/disabled state instead of the whole list.
  returningBorrowId: number | null = null;
  actionError = '';

  ngOnInit(): void {
    this.loadStudentData();
    this.loadStudentBorrows();
  }

  loadStudentData(): void {
    this.studentName = this.authService.getUserName('Student');
    this.studentEmail = this.authService.getUserEmail('');
  }

  loadStudentBorrows(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.borrowService.getBorrowsForStudent().subscribe({
      next: (borrows) => {
        this.borrows = borrows;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load borrowed books', err);
        this.errorMessage = extractErrorMessage(err, 'Failed to load your borrowed books. Please try again later.');
        this.isLoading = false;
      }
    });
  }

  onReturnBook(borrowId: number): void {
    this.actionError = '';
    this.returningBorrowId = borrowId;

    this.borrowService.returnBook(borrowId).subscribe({
      next: () => {
        this.returningBorrowId = null;
        // Re-fetch rather than patch locally: the server is the source of
        // truth for returnDate (set from its own UtcNow) and status.
        this.loadStudentBorrows();
      },
      error: (err) => {
        this.returningBorrowId = null;
        this.actionError = extractErrorMessage(err, 'Failed to return the book. Please try again.');
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
