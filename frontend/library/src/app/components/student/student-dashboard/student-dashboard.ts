import { Component, HostListener, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { SearchService, SmartSearchResultDto } from '../../../services/search.service';
import { BookService, BookDto } from '../../../services/book.service';
import { BorrowService, SaveBorrowDto } from '../../../services/borrow.service';
import { StudentService, SaveStudentDto } from '../../../services/student.service';
import { CategoryService, CategoryDto } from '../../../services/category.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { extractErrorMessage } from '../../../utils/http-error.util';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule
  ],
  templateUrl: './student-dashboard.html',
  styleUrl: './student-dashboard.css'
})
export class StudentDashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private searchService = inject(SearchService);
  private bookService = inject(BookService);
  private borrowService = inject(BorrowService);
  private studentService = inject(StudentService);
  private categoryService = inject(CategoryService);
  private searchSubject = new Subject<string>();

  studentId: number = 0;
  studentName: string = 'Student';
  studentEmail: string = '';
  studentFaculty: string = '';
  studentMajor: string = '';
  studentPhone: string = '';

  searchQuery: string = '';
  searchResults: SmartSearchResultDto[] = [];
  isSearchLoading = false;

  // ===== Category dropdown + grid (new feature) =====
  categories: CategoryDto[] = [];
  selectedCategoryId: number | null = null;
  categoryBooks: BookDto[] = [];
  isCategoryLoading = false;

  showBorrowModal = false;
  isSavingBorrow = false;
  borrowModalError = '';
  availableBooks: BookDto[] = [];
  borrowForm: SaveBorrowDto = {
    borrowDate: '',
    dueDate: '',
    bookId: 0
  };
  minDueDate: string = '';

  showProfileModal = false;
  isSavingProfile = false;
  profileModalError = '';
  profileForm: SaveStudentDto = {
    name: '',
    email: '',
    faculty: '',
    majorName: '',
    phone: ''
  };

  // Page-level banners: only used for post-action confirmations shown after
  // a modal has already closed, never for in-modal validation/errors.
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void {
    this.loadStudentData();
    this.loadAvailableBooks();
    this.loadCategories();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (!query || !query.trim()) {
          this.searchResults = [];
          this.isSearchLoading = false;
          return [];
        }
        this.isSearchLoading = true;
        return this.searchService.smartSearch(query);
      })
    ).subscribe({
      next: (results) => {
        this.isSearchLoading = false;
        this.searchResults = results;
      },
      error: (err) => {
        this.isSearchLoading = false;
        console.error('Search error:', err);
      }
    });
  }

  // Close whichever modal is open when the user presses Escape.
  @HostListener('document:keydown.escape')
  onEscapePressed(): void {
    if (this.showBorrowModal) this.closeBorrowModal();
    if (this.showProfileModal) this.closeProfileModal();
  }

  onSearch(queryText: string) {
    this.searchQuery = queryText;
    this.searchSubject.next(queryText);
  }

  // Clicking a search result opens the Borrow modal pre-filled with that
  // book, so the search results are actually actionable.
  // ASSUMPTION: SmartSearchResultDto has an "id" field matching the book's
  // id. Adjust if your DTO uses a different field name.
  onSelectSearchResult(result: any): void {
    this.searchResults = [];
    this.searchQuery = '';

    const matchedBook = this.availableBooks.find(b => b.id === result.id);
    this.openBorrowModal(matchedBook?.id ?? result.id);
  }

  loadStudentData(): void {
    // NOTE: StudentsController.GetAllStudents is [Authorize(Roles = "Librarian")]
    // only, so a Student-authenticated user gets a 403 from it - there is no
    // self-GET endpoint for a student to fetch their own record (only
    // UpdateStudent, which is write-only). So profile info here comes
    // entirely from the JWT claims; faculty/major/phone aren't in the token
    // and stay blank until the student opens "Edit Profile" and fills them in.
    this.studentId = this.authService.getUserId();
    this.studentName = this.authService.getUserName('Student');
    this.studentEmail = this.authService.getUserEmail('');
  }

  loadAvailableBooks(): void {
    this.bookService.getAllBooks({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (books) => {
        this.availableBooks = books.filter(b => b.availableCopies > 0);
      },
      error: (err) => {
        console.error('Failed to load books', err);
        if (err.status === 403 || err.status === 401) {
          this.errorMessage = 'Your session has expired or you do not have access. Please log in again.';
        }
      }
    });
  }

  loadCategories(): void {
    // CategoriesController.GetAllCategories allows both "Librarian" and
    // "Student" roles, so this call is expected to succeed for a student.
    this.categoryService.getAllCategories().subscribe({
      next: (categories: CategoryDto[]) => {
        this.categories = categories;
      },
      error: (err) => {
        console.error('Failed to load categories', err);
        if (err.status === 403 || err.status === 401) {
          this.errorMessage = 'Category browsing is currently unavailable for your account. Please contact support.';
        }
      }
    });
  }

  // Only called when the student picks a category from the dropdown. The
  // grid stays hidden entirely until selectedCategoryId is set (see the
  // *ngIf on .category-books-section in the HTML).
  onCategoryChange(): void {
    if (!this.selectedCategoryId) {
      this.categoryBooks = [];
      return;
    }

    this.isCategoryLoading = true;

    // ASSUMPTION: BookDto has a "categoryId" field. If your API instead
    // supports server-side filtering (e.g. getAllBooks({ categoryId })),
    // swap this for that call instead of filtering client-side.
    this.bookService.getAllBooks({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (books: any[]) => {
        this.categoryBooks = books.filter(b => b.categoryId === this.selectedCategoryId);
        this.isCategoryLoading = false;
      },
      error: (err) => {
        console.error('Failed to load books for category', err);
        this.isCategoryLoading = false;
      }
    });
  }

  openBorrowModal(preselectedBookId?: number): void {
    const today = new Date();
    const defaultDueDate = new Date();
    defaultDueDate.setDate(today.getDate() + 14);

    const todayStr = today.toISOString().split('T')[0];

    this.borrowForm = {
      // No preselected book (opened from the global "Borrow New Book"
      // button) must leave the dropdown on its placeholder, not silently
      // default to the first available book.
      bookId: preselectedBookId ?? 0,
      borrowDate: todayStr,
      dueDate: defaultDueDate.toISOString().split('T')[0]
    };
    this.minDueDate = todayStr;
    this.borrowModalError = '';
    this.showBorrowModal = true;
  }

  closeBorrowModal(): void {
    if (this.isSavingBorrow) return; // don't allow closing mid-save
    this.showBorrowModal = false;
  }

  submitBorrow(): void {
    this.borrowModalError = '';

    if (!this.borrowForm.bookId || !this.borrowForm.dueDate || !this.borrowForm.borrowDate) {
      this.borrowModalError = 'Please select a book, borrow date, and a valid due date.';
      return;
    }

    if (this.borrowForm.dueDate < this.borrowForm.borrowDate) {
      this.borrowModalError = 'Due date cannot be before the borrow date.';
      return;
    }

    this.isSavingBorrow = true;

    this.borrowService.createBorrow(this.borrowForm).subscribe({
      next: () => {
        this.isSavingBorrow = false;
        this.showBorrowModal = false;
        this.loadAvailableBooks();
        this.successMessage = 'Book borrowed successfully!';
        setTimeout(() => (this.successMessage = ''), 4000);
      },
      error: (err) => {
        this.isSavingBorrow = false;
        this.borrowModalError = extractErrorMessage(err, 'Failed to process borrow request.');
      }
    });
  }

  openProfileModal(): void {
    this.profileModalError = '';

    this.profileForm = {
      name: this.studentName,
      email: this.studentEmail,
      faculty: this.studentFaculty,
      majorName: this.studentMajor,
      phone: this.studentPhone
    };

    this.showProfileModal = true;
  }

  closeProfileModal(): void {
    if (this.isSavingProfile) return; // don't allow closing mid-save
    this.showProfileModal = false;
  }

  submitProfileUpdate(): void {
    this.profileModalError = '';

    if (!this.profileForm.name || !this.profileForm.email) {
      this.profileModalError = 'Name and email are required fields.';
      return;
    }

    this.isSavingProfile = true;

    this.studentService.updateStudent(this.profileForm).subscribe({
      next: () => {
        this.studentName = this.profileForm.name;
        this.studentEmail = this.profileForm.email;
        this.studentFaculty = this.profileForm.faculty;
        this.studentMajor = this.profileForm.majorName;
        this.studentPhone = this.profileForm.phone;
        this.isSavingProfile = false;
        this.showProfileModal = false;
        this.successMessage = 'Profile updated successfully!';
        setTimeout(() => (this.successMessage = ''), 4000);
      },
      error: (err) => {
        this.isSavingProfile = false;
        this.profileModalError = extractErrorMessage(err, 'Failed to update profile.');
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
