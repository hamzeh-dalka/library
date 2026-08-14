export interface User {
  id: number;
  userName: string;
  role: 'Admin' | 'Librarian' | 'Student';
  createdAt?: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface RegisterStudentRequest {
  userName: string;
  password: string;
  name: string;
  email: string;
  phone: string;
  faculty: string;
  majorName: string;
}

export interface RegisterLibrarianRequest {
  userName: string;
  password: string;
  name: string;
  email: string;
  phone: string;
}
