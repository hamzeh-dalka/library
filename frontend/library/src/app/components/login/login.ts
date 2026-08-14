import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractErrorMessage } from '../../utils/http-error.util';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup = this.fb.group({
    userName: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  isLoading = false;
  errorMessage = '';

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const credentials = this.loginForm.value;

    this.authService.login(credentials).subscribe({
      next: (response: any) => {
        this.isLoading = false;

        const token = response?.token;
        if (!token) {
          this.errorMessage = 'Invalid response from server.';
          return;
        }

        // Use centralized AuthService role extraction
        const role = this.authService.getUserRole() || 'student';

        if (role === 'admin') {
          this.router.navigateByUrl('/adminDashboard');
        } else if (role === 'librarian') {
          this.router.navigateByUrl('/librarianDashboard');
        } else {
          this.router.navigateByUrl('/studentDashboard');
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = extractErrorMessage(err, 'Invalid username or password. Please try again.');
      }
    });
  }
}
