import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { LibrarianService, LibrarianDto, SaveLibrarianDto } from '../../../services/librarian.service';
import { RegisterLibrarianRequest } from '../../../models/user.model';
import { extractErrorMessage } from '../../../utils/http-error.util';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private librarianService = inject(LibrarianService);

  adminName: string = 'Developer Admin';
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  librarians: LibrarianDto[] = [];

  newLibrarian: SaveLibrarianDto = {
    name: '',
    userName: '',
    password: '',
    email: '',
    phone: ''
  };

  showAddForm = false;

  ngOnInit(): void {
    this.loadAdminData();
    this.fetchAllLibrarians();
  }

  loadAdminData(): void {
    this.adminName = this.authService.getUserName('Admin');
  }

  fetchAllLibrarians(): void {
    this.isLoading = true;
    this.librarianService.getAllLibrarians().subscribe({
      next: (data) => {
        this.librarians = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  toggleAddForm(): void {
    this.showAddForm = !this.showAddForm;
    this.newLibrarian = { name: '', email: '', phone: '', password: '', userName: '' };
  }

  addLibrarian(): void {
    if (!this.newLibrarian.name || !this.newLibrarian.email || !this.newLibrarian.phone || !this.newLibrarian.password || !this.newLibrarian.userName) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.registerLibrarian(this.newLibrarian as RegisterLibrarianRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.showSuccess('Librarian added successfully.');
        this.newLibrarian = { name: '', email: '', phone: '', password: '', userName: '' };
        this.showAddForm = false;
        this.fetchAllLibrarians();
      },
      error: (err) => this.handleError(err)
    });
  }

  deleteLibrarian(id: number): void {
    if (!confirm('Are you sure you want to delete this librarian?')) return;

    this.librarianService.deleteLibrarian(id).subscribe({
      next: () => {
        this.showSuccess('Librarian deleted successfully.');
        this.fetchAllLibrarians();
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
