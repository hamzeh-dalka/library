import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { BookService, BookDto } from '../../../services/book.service';
import { CategoryService, CategoryDto } from '../../../services/category.service';
import { BorrowService, BorrowDto } from '../../../services/borrow.service';
import { LibrarianService, UpdateLibrarianDto } from '../../../services/librarian.service';
import { BorrowStatus } from '../../../models/borrow-status.model';
import { extractErrorMessage } from '../../../utils/http-error.util';

@Component({
  selector: 'app-librarian-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './librarian-dashboard.html',
  styleUrl: './librarian-dashboard.css'
})
export class LibrarianDashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private bookService = inject(BookService);
  private categoryService = inject(CategoryService);
  private borrowService = inject(BorrowService);
  private librarianService = inject(LibrarianService);

  librarianId: number = 0;
  librarianName: string = 'Librarian';
  librarianEmail: string = '';
  librarianPhone: string = '';

  // Split from dashboard stats loading, so the profile card isn't hidden
  // just because the statistics are still loading.
  isProfileLoading = false;
  isStatsLoading = false;

  totalBooks: number = 0;
  totalCategories: number = 0;
  totalBorrows: number = 0;
  activeBorrows: number = 0;
  returnedBorrows: number = 0;

  showProfileModal = false;
  isSaving = false;
  profileForm: UpdateLibrarianDto = {
    name: '',
    email: '',
    phone: ''
  };

  successMessage = '';
  errorMessage = '';

  // Modal-local error, shown inside the dialog itself so it's visible while
  // the modal is open (page-level alerts sit behind the modal backdrop).
  modalErrorMessage = '';

  ngOnInit(): void {
    this.loadLibrarianData();
    this.loadDashboardStatistics();
  }

  loadLibrarianData(): void {
    // NOTE: LibrariansController.GetAllLibrarians is [Authorize(Roles = "Admin")]
    // only, so a Librarian-authenticated user gets a 403 from it - there is no
    // self-GET endpoint for a librarian to fetch their own record (only
    // UpdateLibrarian, which is write-only). So profile info here comes
    // entirely from the JWT claims; phone isn't in the token and stays blank
    // until the librarian opens "Edit Profile" and enters it themselves.
    this.librarianId = this.authService.getUserId();
    this.librarianName = this.authService.getUserName('Librarian');
    this.librarianEmail = this.authService.getUserEmail('');
    this.isProfileLoading = false;
  }

  loadDashboardStatistics(): void {
    this.isStatsLoading = true;

    this.bookService.getAllBooks({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (books: BookDto[]) => {
        this.totalBooks = Array.isArray(books) ? books.length : 0;
      },
      error: (err) => console.error(err)
    });

    this.categoryService.getAllCategories().subscribe({
      next: (categories: CategoryDto[]) => {
        this.totalCategories = Array.isArray(categories) ? categories.length : 0;
      },
      error: (err) => console.error(err)
    });

    this.borrowService.getAllBorrows({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (borrows: BorrowDto[]) => {
        this.totalBorrows = Array.isArray(borrows) ? borrows.length : 0;
        this.activeBorrows = Array.isArray(borrows)
          ? borrows.filter(b => b.status === BorrowStatus.Borrowed || b.status === BorrowStatus.Overdue).length
          : 0;
        this.returnedBorrows = Array.isArray(borrows)
          ? borrows.filter(b => b.status === BorrowStatus.Returned).length
          : 0;
        this.isStatsLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isStatsLoading = false;
      }
    });
  }

  openProfileModal(): void {
    this.successMessage = '';
    this.errorMessage = '';
    this.modalErrorMessage = '';

    this.profileForm = {
      name: this.librarianName,
      email: this.librarianEmail,
      phone: this.librarianPhone
    };

    this.showProfileModal = true;
  }

  closeProfileModal(): void {
    if (this.isSaving) return; // don't allow closing mid-save
    this.showProfileModal = false;
  }

  submitProfileUpdate(): void {
    this.modalErrorMessage = '';

    if (!this.profileForm.name || !this.profileForm.email) {
      this.modalErrorMessage = 'Name and email are required fields.';
      return;
    }

    this.isSaving = true;

    this.librarianService.updateLibrarian(this.profileForm).subscribe({
      next: () => {
        this.librarianName = this.profileForm.name;
        this.librarianEmail = this.profileForm.email;
        this.librarianPhone = this.profileForm.phone;
        this.isSaving = false;
        this.showProfileModal = false;
        this.successMessage = 'Profile updated successfully!';
        setTimeout(() => (this.successMessage = ''), 4000);
      },
      error: (err) => {
        this.isSaving = false;
        this.modalErrorMessage = extractErrorMessage(err, 'Failed to update profile.');
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
