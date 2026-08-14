# Library Management System

A full-stack university library management system built with **ASP.NET Core Web API** (backend) and **Angular** (frontend). The system supports three roles — Admin, Librarian, and Student — each with a dedicated dashboard and permission-scoped features.

## Features

- Role-based authentication and authorization (Admin / Librarian / Student) using JWT
- Book catalog with category management, search, and filtering
- AI-powered semantic ("smart") search using vector embeddings
- Book borrowing workflow with due dates, return tracking, and overdue status
- Librarian tools for managing books, categories, and borrow records
- Admin tools for managing librarian accounts
- Student self-service: browse books, request borrows, track return status, update profile

## Tech Stack

- **Backend:** ASP.NET Core Web API, Entity Framework Core, JWT Authentication, BCrypt
- **Frontend:** Angular
- **Database:** SQL Server (via EF Core)

## API Endpoints

All endpoints are prefixed with `/api`. Endpoints marked 🔒 require a valid JWT (`Authorization: Bearer <token>`), with the required role(s) noted.

### Auth (`/api/Auth`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/RegisterLibrarian` | 🔒 Admin | Register a new librarian account |
| POST | `/RegisterStudent` | Public | Register a new student account |
| POST | `/Login` | Public | Authenticate and receive a JWT |

### Books (`/api/Books`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/GetAllBooks` | 🔒 Librarian, Student | List/search/filter books (paginated) |
| POST | `/AddBook` | 🔒 Librarian | Add a new book |
| PUT | `/UpdateBook/{id}` | 🔒 Librarian | Update an existing book |
| DELETE | `/DeleteBook/{id}` | 🔒 Librarian | Delete a book (blocked if active borrows exist) |
| GET | `/GetRecommendedBooks` | 🔒 Student | AI-powered recommendations based on last borrow |

### Borrows (`/api/Borrows`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/GetAllBorrows` | 🔒 Librarian | List/filter all borrow records (paginated) |
| GET | `/GetBorrowsForStudent` | 🔒 Student | Get the current student's borrow records |
| POST | `/CreateBorrow` | 🔒 Student | Request to borrow a book |
| PATCH | `/ReturnBook/{id}` | 🔒 Student | Mark a borrowed book as returned |
| PATCH | `/ExtendDueDate/{id}` | 🔒 Student | Extend the due date of an active borrow |
| DELETE | `/DeleteBorrow/{id}` | 🔒 Librarian | Delete a borrow record |

### Categories (`/api/Categories`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/GetAllCategories` | 🔒 Librarian, Student | List/search categories |
| POST | `/AddCategory` | 🔒 Librarian | Add a new category |
| PUT | `/UpdateCategory` | 🔒 Librarian | Update a category name |
| DELETE | `/DeleteCategory` | 🔒 Librarian | Delete a category (blocked if books are assigned to it) |

### Librarians (`/api/Librarians`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/GetAllLibrarians` | 🔒 Admin | List/search librarians |
| PUT | `/UpdateLibrarian` | 🔒 Librarian | Update the current librarian's own profile |
| DELETE | `/DeleteLibrarian` | 🔒 Admin | Delete a librarian account |

### Students (`/api/Students`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/GetAllStudents` | 🔒 Librarian | List/search students |
| PUT | `/UpdateStudent` | 🔒 Student | Update the current student's own profile |
| DELETE | `/DeleteStudent` | 🔒 Librarian | Delete a student account |

### Search (`/api/Search`)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/smart-search` | 🔒 Student | Semantic book search using AI embeddings |

## Screenshots

### Admin

| Admin Dashboard |
|---|
| ![Admin Dashboard](./src/screenshots/adminDashboard.png) |

### Librarian

| Librarian Dashboard | Manage Books & Categories |
|---|---|
| ![Librarian Dashboard](./src/screenshots/Librarian%20Dashboard.png) | ![Manage Books and Categories](./src/screenshots/manageBooksAndCategorys.png) |

| Add New Book | Edit Book |
|---|---|
| ![Add New Book](./src/screenshots/addNewBook.png) | ![Edit Book](./src/screenshots/editBook.png) |

| Manage Borrows | Update Profile |
|---|---|
| ![Manage Borrows](./src/screenshots/manageBorrows.png) | ![Librarian Update Profile](./src/screenshots/librarianUpdateProfile.png) |

### Student

| Student Dashboard | Request Borrow |
|---|---|
| ![Student Dashboard](./src/screenshots/Student%20Dashboard.png) | ![Request Borrow](./src/screenshots/requestBoorow.png) |

| Student Borrows | Update Profile |
|---|---|
| ![Student Borrows](./src/screenshots/Student%20Borrows.png) | ![Student Update Profile](./src/screenshots/studentUpdateProfile.png) |

## Getting Started

### Prerequisites

- .NET SDK
- Node.js and npm
- SQL Server (or your configured EF Core provider)

The app will be available at `http://localhost:4200`, with the API running separately (see `environment.ts` for the configured API base URL).

## Project Structure

```
src/
├── app/                # Angular application source
├── environments/       # Environment configuration
├── screenshots/        # README screenshots
├── index.html
├── main.ts
└── styles.css
```
