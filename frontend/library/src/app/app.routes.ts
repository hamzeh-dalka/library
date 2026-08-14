import { Routes } from '@angular/router';
import { roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/splash-screen/splash-screen').then(m => m.SplashScreenComponent),
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/register-student/register-student').then(m => m.RegisterStudentComponent)
  },

  {
    path: 'studentDashboard',
    loadComponent: () =>
      import('./components/student/student-dashboard/student-dashboard').then(m => m.StudentDashboardComponent),
    canActivate: [roleGuard(['student'])]
  },
  {
    path: 'studentBorrows',
    loadComponent: () =>
      import('./components/student/student-borrows/student-borrows').then(m => m.StudentBorrowsComponent),
    canActivate: [roleGuard(['student'])]
  },

  {
    path: 'librarianDashboard',
    loadComponent: () =>
      import('./components/librarian/librarian-dashboard/librarian-dashboard').then(m => m.LibrarianDashboardComponent),
    canActivate: [roleGuard(['librarian'])]
  },
  {
    path: 'librarianBooks',
    loadComponent: () =>
      import('./components/librarian/librarian-books/librarian-books').then(m => m.LibrarianBooksComponent),
    canActivate: [roleGuard(['librarian'])]
  },
  {
    path: 'librarianBorrows',
    loadComponent: () =>
      import('./components/librarian/librarian-borrows/librarian-borrows').then(m => m.LibrarianBorrowsComponent),
    canActivate: [roleGuard(['librarian'])]
  },

  {
    path: 'adminDashboard',
    loadComponent: () =>
      import('./components/admin/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardComponent),
    canActivate: [roleGuard(['admin'])]
  },

  {
    path: '**',
    redirectTo: ''
  }
];
