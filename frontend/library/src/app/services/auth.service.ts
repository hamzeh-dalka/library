import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterStudentRequest,
  RegisterLibrarianRequest
} from '../models/user.model';

export interface DecodedTokenPayload {
  nameidentifier?: string;
  sub?: string;
  name?: string;
  unique_name?: string;
  emailaddress?: string;
  email?: string;
  role?: string;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Auth`;

  // Token lives in localStorage rather than an httpOnly cookie. An httpOnly
  // cookie would be the safer choice against XSS token theft, but it requires
  // backend changes (Set-Cookie on login, cookie-based auth scheme, CORS
  // AllowCredentials) that are out of scope here - the API is a fixed
  // contract. Tradeoff: the token is readable by any script running on this
  // origin, so it's only as safe as this app staying free of XSS.
  private readonly tokenKey = 'authToken';

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/Login`, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          this.setToken(response.token);
        }
      })
    );
  }

  registerStudent(studentData: RegisterStudentRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/RegisterStudent`, studentData);
  }

  registerLibrarian(librarianData: RegisterLibrarianRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/RegisterLibrarian`, librarianData);
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
    }
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  setToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  /**
   * Centralized JWT decoding helper
   */
  getDecodedToken(): DecodedTokenPayload | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      // Base64Url decode with UTF-8 handling
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      return JSON.parse(jsonPayload);
    } catch (e) {
      console.error('Failed to decode JWT token:', e);
      return null;
    }
  }

  /**
   * Extracts normalized role ('admin', 'librarian', 'student')
   */
  getUserRole(): string | null {
    const payload = this.getDecodedToken();
    if (!payload) return null;

    // استخدام الـ Bracket Notation لتجنب أخطاء الـ Index Signature
    const rawRole =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      payload['role'] ||
      payload['Role'];

    return rawRole ? String(rawRole).toLowerCase() : null;
  }

  /**
   * Extracts user display name
   */
  getUserName(defaultName: string = 'User'): string {
    const payload = this.getDecodedToken();
    if (!payload) return defaultName;

    return (
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
      payload['unique_name'] ||
      payload['name'] ||
      defaultName
    );
  }

  /**
   * Extracts user email
   */
  getUserEmail(defaultEmail: string = ''): string {
    const payload = this.getDecodedToken();
    if (!payload) return defaultEmail;

    return (
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
      payload['email'] ||
      defaultEmail
    );
  }

  /**
   * Extracts user ID
   */
  getUserId(): number {
    const payload = this.getDecodedToken();
    if (!payload) return 0;

    const rawId =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      payload['sub'] ||
      payload['id'] ||
      0;

    return Number(rawId) || 0;
  }

  /**
   * Checks if current logged in user has a required role
   */
  hasRole(allowedRoles: string | string[]): boolean {
    const currentRole = this.getUserRole();
    if (!currentRole) return false;

    const rolesArray = Array.isArray(allowedRoles)
      ? allowedRoles.map(r => r.toLowerCase())
      : [allowedRoles.toLowerCase()];

    return rolesArray.includes(currentRole);
  }
}
