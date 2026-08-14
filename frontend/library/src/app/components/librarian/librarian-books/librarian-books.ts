import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BookService, BookDto, SaveBookDto } from '../../../services/book.service';
import { CategoryService, CategoryDto, SaveCategoryDto } from '../../../services/category.service';
import { AuthService } from '../../../services/auth.service';
import { RouterModule, Router } from '@angular/router';
import { extractErrorMessage } from '../../../utils/http-error.util';

@Component({
  selector: 'app-librarian-books',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './librarian-books.html',
  styleUrl: './librarian-books.css'
})
export class LibrarianBooksComponent implements OnInit {
  private bookService = inject(BookService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);
  private router = inject(Router);
  librarianName: string = 'Librarian';
  books: BookDto[] = [];
  categories: CategoryDto[] = [];
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  // التحكم في نافذة نموذج الكتب (Modal)
  showBookModal = false;
  isEditMode = false;
  currentBookId: number | null = null;
  bookForm: SaveBookDto = {
    title: '',
    author: '',
    totalCopies: 1,
    publishedYear: 2026,
    categoryId: 0
  };

  // التحكم في نموذج الفئات السريع
  showCategorySection = false;
  newCategory: SaveCategoryDto = { name: '' };

  ngOnInit(): void {
    this.loadLibrarianData();
    this.loadData();
  }

  loadLibrarianData(): void {
    this.librarianName = this.authService.getUserName('Librarian');
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.bookService.getAllBooks({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (data) => {
        this.books = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err)
    });

    this.categoryService.getAllCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => this.handleError(err)
    });
  }

  openAddBookModal(): void {
    this.isEditMode = false;
    this.currentBookId = null;
    this.bookForm = {
      title: '',
      author: '',
      totalCopies: 1,
      publishedYear: 2026,
      categoryId: this.categories[0]?.id || 0
    };
    this.showBookModal = true;
  }

  openEditBookModal(book: BookDto): void {
    this.isEditMode = true;
    this.currentBookId = book.id;
    this.bookForm = {
      title: book.title,
      author: book.author,
      totalCopies: book.totalCopies,
      availableCopies: book.availableCopies,
      publishedYear: book.publishedYear,
      categoryId: book.categoryId
    };
    this.showBookModal = true;
  }

  saveBook(): void {
    if (!this.bookForm.title || !this.bookForm.author || !this.bookForm.categoryId) {
      this.errorMessage = 'Please fill in all required book fields.';
      return;
    }

    if (this.isEditMode && this.currentBookId !== null) {
      this.bookService.updateBook(this.currentBookId, this.bookForm).subscribe({
        next: () => {
          this.showSuccess('Book updated successfully.');
          this.showBookModal = false;
          this.loadData();
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.bookService.addBook(this.bookForm).subscribe({
        next: () => {
          this.showSuccess('Book added successfully.');
          this.showBookModal = false;
          this.loadData();
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  deleteBook(id: number): void {
    if (!confirm('Are you sure you want to delete this book?')) return;

    this.bookService.deleteBook(id).subscribe({
      next: () => {
        this.showSuccess('Book deleted successfully.');
        this.loadData();
      },
      error: (err) => this.handleError(err)
    });
  }

  deleteCategory(id: number): void {
    if (!confirm('Are you sure you want to delete this category?')) return;

    this.categoryService.deleteCategory(id).subscribe({
      next: () => {
        this.showSuccess('Category deleted successfully.');
        this.loadData();
      },
      error: (err) => this.handleError(err)
    });
  }

  addCategory(): void {
    if (!this.newCategory.name.trim()) return;

    this.categoryService.addCategory(this.newCategory).subscribe({
      next: () => {
        this.showSuccess('Category added successfully.');
        this.newCategory.name = '';
        this.loadData();
      },
      error: (err) => this.handleError(err)
    });
  }

  private showSuccess(msg: string): void {
    this.successMessage = msg;
    setTimeout(() => this.successMessage = '', 4000);
  }

  private handleError(err: any): void {
    this.isLoading = false;
    console.error(err);
    this.errorMessage = extractErrorMessage(err, 'An unexpected error occurred.');
    setTimeout(() => this.errorMessage = '', 4000);
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
